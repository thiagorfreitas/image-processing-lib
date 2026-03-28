using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageProcessingEngine.Tests.Fixtures;

/// <summary>
/// Generates small synthetic images in-memory for use in unit tests.
/// No external image files are needed — tests are fully self-contained.
/// </summary>
public static class TestImageFactory
{
    /// <summary>
    /// Creates a solid-colour JPEG image of the given dimensions.
    /// Default: 200 × 150 px.
    /// </summary>
    public static byte[] CreateJpeg(int width = 200, int height = 150)
    {
        using var img = new Image<Rgb24>(width, height);
        using var ms  = new MemoryStream();
        img.SaveAsJpeg(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Creates a solid-colour PNG image (with alpha channel) of the given dimensions.
    /// Default: 200 × 150 px.
    /// </summary>
    public static byte[] CreatePng(int width = 200, int height = 150)
    {
        using var img = new Image<Rgba32>(width, height);
        using var ms  = new MemoryStream();
        img.SaveAsPng(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// Creates a solid-colour WebP image of the given dimensions.
    /// Default: 200 × 150 px.
    /// </summary>
    public static byte[] CreateWebp(int width = 200, int height = 150)
    {
        using var img = new Image<Rgb24>(width, height);
        using var ms  = new MemoryStream();
        img.SaveAsWebp(ms);
        return ms.ToArray();
    }
}
