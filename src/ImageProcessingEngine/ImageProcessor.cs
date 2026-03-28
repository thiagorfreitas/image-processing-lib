using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace ImageProcessingEngine;

/// <summary>
/// Concrete implementation of <see cref="IImageProcessor"/>.
///
/// Design decisions:
/// - All operations work entirely in-memory (MemoryStream) — no temp files,
///   safe for serverless/container execution on ODC.
/// - SixLabors.ImageSharp is 100% managed C# — no native binaries required,
///   compatible with ODC's linux-x64 containers.
/// - ILogger is injected by ODC's runtime. Logs appear in the ODC Portal.
/// - Distributed tracing uses Activity.Current?.Source.StartActivity so spans
///   appear in ODC's trace viewer.
/// - Guard() validates all inputs before any image work begins; this surfaces
///   descriptive errors directly in the ODC Server Action error handler.
/// </summary>
public class ImageProcessor : IImageProcessor
{
    // ─── Tracing source name (matches what ODC expects) ──────────────────────
    private static readonly ActivitySource _activitySource =
        new("ImageProcessingEngine.ImageProcessor");

    private readonly ILogger _logger;

    // Default JPEG quality used by GenerateThumbnail
    private const int DefaultThumbnailQuality = 85;

    public ImageProcessor(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ResizeImage
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public byte[] ResizeImage(byte[] image, int width, int height)
    {
        using var activity = _activitySource.StartActivity("ImageProcessor.ResizeImage");
        activity?.SetTag("target.width", width);
        activity?.SetTag("target.height", height);

        Guard.NotNullOrEmpty(image,  nameof(image));
        Guard.Positive(width,  nameof(width),  "Width must be greater than 0.");
        Guard.Positive(height, nameof(height), "Height must be greater than 0.");

        _logger.LogInformation("ResizeImage called. Target: {Width}x{Height}", width, height);

        using var inputStream  = new MemoryStream(image);
        var       formatInfo   = Image.DetectFormat(inputStream);
        inputStream.Position   = 0;

        using var img = Image.Load(inputStream);
        img.Mutate(ctx => ctx.Resize(width, height));

        return EncodeToBytes(img, formatInfo);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CropImage
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public byte[] CropImage(byte[] image, int x, int y, int width, int height)
    {
        using var activity = _activitySource.StartActivity("ImageProcessor.CropImage");
        activity?.SetTag("crop.x", x);
        activity?.SetTag("crop.y", y);
        activity?.SetTag("crop.width", width);
        activity?.SetTag("crop.height", height);

        Guard.NotNullOrEmpty(image, nameof(image));
        Guard.NotNegative(x, nameof(x), "X coordinate must be >= 0.");
        Guard.NotNegative(y, nameof(y), "Y coordinate must be >= 0.");
        Guard.Positive(width,  nameof(width),  "Crop width must be greater than 0.");
        Guard.Positive(height, nameof(height), "Crop height must be greater than 0.");

        _logger.LogInformation(
            "CropImage called. Rect: ({X},{Y}) {Width}x{Height}", x, y, width, height);

        using var inputStream = new MemoryStream(image);
        var       formatInfo  = Image.DetectFormat(inputStream);
        inputStream.Position  = 0;

        using var img = Image.Load(inputStream);

        // Validate that the crop rectangle fits within the source image.
        if (x + width > img.Width || y + height > img.Height)
        {
            throw new ArgumentException(
                $"Crop rectangle ({x},{y} {width}x{height}) exceeds image bounds ({img.Width}x{img.Height}).");
        }

        var cropRect = new Rectangle(x, y, width, height);
        img.Mutate(ctx => ctx.Crop(cropRect));

        return EncodeToBytes(img, formatInfo);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ConvertImageFormat
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public byte[] ConvertImageFormat(byte[] image, string targetFormat)
    {
        using var activity = _activitySource.StartActivity("ImageProcessor.ConvertImageFormat");
        activity?.SetTag("target.format", targetFormat);

        Guard.NotNullOrEmpty(image, nameof(image));

        if (string.IsNullOrWhiteSpace(targetFormat))
            throw new ArgumentException("Target format must not be null or empty.", nameof(targetFormat));

        _logger.LogInformation("ConvertImageFormat called. Target format: {Format}", targetFormat);

        var encoder = ResolveEncoder(targetFormat.Trim().ToLowerInvariant());

        using var inputStream = new MemoryStream(image);
        using var img         = Image.Load(inputStream);
        using var outputStream = new MemoryStream();

        img.Save(outputStream, encoder);
        return outputStream.ToArray();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CompressImage
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public byte[] CompressImage(byte[] image, int quality)
    {
        using var activity = _activitySource.StartActivity("ImageProcessor.CompressImage");
        activity?.SetTag("jpeg.quality", quality);

        Guard.NotNullOrEmpty(image, nameof(image));

        if (quality < 1 || quality > 100)
            throw new ArgumentOutOfRangeException(
                nameof(quality), quality, "Quality must be between 1 and 100 inclusive.");

        _logger.LogInformation("CompressImage called. Quality: {Quality}", quality);

        var encoder         = new JpegEncoder { Quality = quality };

        using var inputStream  = new MemoryStream(image);
        using var img          = Image.Load(inputStream);
        using var outputStream = new MemoryStream();

        img.Save(outputStream, encoder);
        return outputStream.ToArray();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GenerateThumbnail
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public byte[] GenerateThumbnail(byte[] image, int maxSize)
    {
        using var activity = _activitySource.StartActivity("ImageProcessor.GenerateThumbnail");
        activity?.SetTag("thumbnail.maxSize", maxSize);

        Guard.NotNullOrEmpty(image, nameof(image));
        Guard.Positive(maxSize, nameof(maxSize), "MaxSize must be greater than 0.");

        _logger.LogInformation("GenerateThumbnail called. MaxSize: {MaxSize}", maxSize);

        using var inputStream = new MemoryStream(image);
        using var img         = Image.Load(inputStream);

        // Scale so the longest side == maxSize, preserving aspect ratio.
        var resizeOptions = new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(maxSize, maxSize)
        };

        img.Mutate(ctx => ctx.Resize(resizeOptions));

        var encoder         = new JpegEncoder { Quality = DefaultThumbnailQuality };
        using var outputStream = new MemoryStream();
        img.Save(outputStream, encoder);

        return outputStream.ToArray();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Re-encodes <paramref name="img"/> using the provided <paramref name="format"/>,
    /// preserving the original format (JPEG→JPEG, PNG→PNG, etc.).
    /// </summary>
    private static byte[] EncodeToBytes(Image img, IImageFormat format)
    {
        using var ms = new MemoryStream();
        img.Save(ms, format);
        return ms.ToArray();
    }

    /// <summary>
    /// Maps a lowercase format string to the appropriate ImageSharp encoder.
    /// Throws <see cref="ArgumentException"/> for unsupported values.
    /// </summary>
    private static IImageEncoder ResolveEncoder(string format) =>
        format switch
        {
            "jpeg" or "jpg" => new JpegEncoder(),
            "png"           => new PngEncoder(),
            "webp"          => new WebpEncoder(),
            _               => throw new ArgumentException(
                                   $"Unsupported target format '{format}'. " +
                                   "Accepted values: 'jpeg', 'jpg', 'png', 'webp'.",
                                   nameof(format))
        };
}
