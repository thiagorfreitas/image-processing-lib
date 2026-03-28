using OutSystems.ExternalLibraries.SDK;

namespace ImageProcessingEngine;

/// <summary>
/// Exposes server-side image manipulation as OutSystems Server Actions.
/// Each method maps 1-to-1 to an ODC Server Action once the ZIP is uploaded
/// to the ODC Portal as an External Library.
///
/// Supported input formats (auto-detected): JPEG, PNG, BMP, GIF, TIFF, WebP.
/// </summary>
[OSInterface(
    Name        = "ImageProcessor",
    Description = "Server-side image manipulation: resize, crop, format conversion, compression, and thumbnail generation.")]
public interface IImageProcessor
{
    /// <summary>
    /// Resizes an image to exactly the specified pixel dimensions.
    /// The output uses the same format as the input (JPEG → JPEG, PNG → PNG, etc.).
    /// NOTE: This stretches the image to the exact target size — aspect ratio is not preserved.
    ///       If you need aspect-ratio-safe scaling, consider using GenerateThumbnail instead.
    /// </summary>
    [OSAction(
        Description = "Resizes an image to the given width and height in pixels. " +
                      "The output format matches the input format.",
        ReturnName  = "ResizedImage")]
    byte[] ResizeImage(
        [OSParameter(Description = "Raw image bytes in any supported format (JPEG, PNG, BMP, GIF, TIFF, WebP).")] byte[] image,
        [OSParameter(Description = "Target width in pixels. Must be greater than 0.")] int width,
        [OSParameter(Description = "Target height in pixels. Must be greater than 0.")] int height);

    /// <summary>
    /// Crops a rectangular region out of an image.
    /// The crop rectangle is defined by its top-left corner (x, y) and its dimensions (width, height).
    /// The output format matches the input format.
    /// </summary>
    [OSAction(
        Description = "Crops a rectangular region from an image. (x, y) is the top-left corner of the crop area.",
        ReturnName  = "CroppedImage")]
    byte[] CropImage(
        [OSParameter(Description = "Raw image bytes in any supported format.")] byte[] image,
        [OSParameter(Description = "X coordinate (in pixels) of the top-left corner of the crop region. Must be >= 0.")] int x,
        [OSParameter(Description = "Y coordinate (in pixels) of the top-left corner of the crop region. Must be >= 0.")] int y,
        [OSParameter(Description = "Width of the crop region in pixels. Must be > 0.")] int width,
        [OSParameter(Description = "Height of the crop region in pixels. Must be > 0.")] int height);

    /// <summary>
    /// Converts an image to a different file format.
    /// Accepted values for targetFormat: "jpeg", "jpg", "png", "webp" (case-insensitive).
    /// </summary>
    [OSAction(
        Description = "Converts an image to the specified format. Accepted values: 'jpeg', 'jpg', 'png', 'webp'.",
        ReturnName  = "ConvertedImage")]
    byte[] ConvertImageFormat(
        [OSParameter(Description = "Raw image bytes in any supported format.")] byte[] image,
        [OSParameter(Description = "Target format string: 'jpeg', 'jpg', 'png', or 'webp' (case-insensitive).")] string targetFormat);

    /// <summary>
    /// Re-encodes an image as JPEG at the specified quality level.
    /// Lower quality = smaller file size.  Quality must be between 1 and 100 inclusive.
    /// </summary>
    [OSAction(
        Description = "Compresses an image as JPEG at the given quality (1 = lowest quality / smallest file, 100 = highest quality / largest file).",
        ReturnName  = "CompressedImage")]
    byte[] CompressImage(
        [OSParameter(Description = "Raw image bytes in any supported format.")] byte[] image,
        [OSParameter(Description = "JPEG quality level from 1 (lowest) to 100 (highest).")] int quality);

    /// <summary>
    /// Creates a thumbnail by scaling the image so that its longest side equals maxSize pixels.
    /// Aspect ratio is always preserved. The output is encoded as JPEG.
    /// </summary>
    [OSAction(
        Description = "Creates a thumbnail where the longest dimension is scaled down to maxSize pixels. Aspect ratio is preserved. Output format: JPEG.",
        ReturnName  = "ThumbnailImage")]
    byte[] GenerateThumbnail(
        [OSParameter(Description = "Raw image bytes in any supported format.")] byte[] image,
        [OSParameter(Description = "Maximum side length (width or height) in pixels. Must be > 0.")] int maxSize);
}
