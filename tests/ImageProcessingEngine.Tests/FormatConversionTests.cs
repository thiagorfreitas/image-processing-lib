using ImageProcessingEngine.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using Xunit;

namespace ImageProcessingEngine.Tests;

/// <summary>
/// TDD tests for <see cref="ImageProcessor.ConvertImageFormat"/>.
/// Format identity is verified via magic bytes (file signatures) rather than
/// MIME types, making the assertions format-library-agnostic.
/// </summary>
public class FormatConversionTests
{
    private readonly ImageProcessor _processor = new(NullLogger.Instance);

    // Magic byte sequences for format detection
    private static readonly byte[] JpegMagic = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngMagic  = [0x89, 0x50, 0x4E, 0x47]; // ‌PNG\r
    private static readonly byte[] WebpRiff  = [0x52, 0x49, 0x46, 0x46]; // RIFF

    // ── Happy-path ────────────────────────────────────────────────────────────

    [Fact]
    public void ConvertToJpeg_ShouldProduceJpegBytes()
    {
        byte[] source = TestImageFactory.CreatePng();

        byte[] result = _processor.ConvertImageFormat(source, "jpeg");

        Assert.True(StartsWith(result, JpegMagic), "Output should start with JPEG magic bytes FF D8 FF.");
    }

    [Fact]
    public void ConvertToJpg_AliasAlsoProducesJpegBytes()
    {
        byte[] source = TestImageFactory.CreatePng();

        byte[] result = _processor.ConvertImageFormat(source, "jpg");

        Assert.True(StartsWith(result, JpegMagic));
    }

    [Fact]
    public void ConvertToPng_ShouldProducePngBytes()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.ConvertImageFormat(source, "png");

        Assert.True(StartsWith(result, PngMagic), "Output should start with PNG magic bytes 89 50 4E 47.");
    }

    [Fact]
    public void ConvertToWebp_ShouldProduceWebPBytes()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.ConvertImageFormat(source, "webp");

        // WebP files start with the RIFF header "RIFF....WEBP"
        Assert.True(StartsWith(result, WebpRiff), "Output should start with RIFF (WebP container).");
        // Bytes 8–11 should be 'W','E','B','P'
        Assert.Equal("WEBP", System.Text.Encoding.ASCII.GetString(result, 8, 4));
    }

    [Fact]
    public void ConvertImageFormat_IsCaseInsensitive()
    {
        byte[] source = TestImageFactory.CreatePng();

        byte[] result = _processor.ConvertImageFormat(source, "JPEG");

        Assert.True(StartsWith(result, JpegMagic));
    }

    [Fact]
    public void ConvertImageFormat_OutputShouldBeDecodable()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.ConvertImageFormat(source, "png");

        var exception = Record.Exception(() => Image.Load(result));
        Assert.Null(exception);
    }

    // ── Guard / validation ────────────────────────────────────────────────────

    [Fact]
    public void ConvertImageFormat_WithNullBytes_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _processor.ConvertImageFormat(null!, "jpeg"));
    }

    [Fact]
    public void ConvertImageFormat_WithEmptyBytes_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _processor.ConvertImageFormat(Array.Empty<byte>(), "jpeg"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ConvertImageFormat_WithEmptyOrWhitespaceFormat_ShouldThrowArgumentException(
        string format)
    {
        byte[] source = TestImageFactory.CreateJpeg();
        Assert.Throws<ArgumentException>(() =>
            _processor.ConvertImageFormat(source, format));
    }

    [Theory]
    [InlineData("bmp")]
    [InlineData("tiff")]
    [InlineData("gif")]
    [InlineData("svg")]
    [InlineData("random")]
    public void ConvertImageFormat_WithUnsupportedFormat_ShouldThrowArgumentException(
        string format)
    {
        byte[] source = TestImageFactory.CreateJpeg();
        Assert.Throws<ArgumentException>(() =>
            _processor.ConvertImageFormat(source, format));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool StartsWith(byte[] data, byte[] prefix)
    {
        if (data.Length < prefix.Length) return false;
        for (int i = 0; i < prefix.Length; i++)
            if (data[i] != prefix[i]) return false;
        return true;
    }
}
