using ImageProcessingEngine.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using Xunit;

namespace ImageProcessingEngine.Tests;

/// <summary>
/// TDD tests for <see cref="ImageProcessor.CompressImage"/>.
///
/// Key assertion strategy: CompressImage always encodes as JPEG.
/// File-size reduction is validated by comparing quality=10 output vs quality=95 output
/// from the same source — the low-quality result must be smaller.
/// </summary>
public class CompressionTests
{
    private readonly ImageProcessor _processor = new(NullLogger.Instance);

    // ── Happy-path ────────────────────────────────────────────────────────────

    [Fact]
    public void CompressImage_LowQuality_ShouldProduceSmallerFileThanHighQuality()
    {
        // Use a large enough source so the quality difference is visible.
        byte[] source = TestImageFactory.CreateJpeg(800, 600);

        byte[] highQuality = _processor.CompressImage(source, quality: 95);
        byte[] lowQuality  = _processor.CompressImage(source, quality: 10);

        Assert.True(
            lowQuality.Length < highQuality.Length,
            $"Expected low-quality ({lowQuality.Length} bytes) < high-quality ({highQuality.Length} bytes).");
    }

    [Fact]
    public void CompressImage_Quality100_ShouldReturnValidImage()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.CompressImage(source, quality: 100);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        var exception = Record.Exception(() => Image.Load(result));
        Assert.Null(exception);
    }

    [Fact]
    public void CompressImage_Quality1_ShouldReturnValidImage()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.CompressImage(source, quality: 1);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        var exception = Record.Exception(() => Image.Load(result));
        Assert.Null(exception);
    }

    [Fact]
    public void CompressImage_OutputShouldBeDecodable()
    {
        byte[] source = TestImageFactory.CreatePng(); // input is PNG; output should be JPEG

        byte[] result = _processor.CompressImage(source, quality: 75);

        var exception = Record.Exception(() => Image.Load(result));
        Assert.Null(exception);
    }

    [Fact]
    public void CompressImage_OutputAlwaysEncodesAsJpeg()
    {
        byte[] source = TestImageFactory.CreatePng();

        byte[] result = _processor.CompressImage(source, quality: 80);

        // JPEG magic bytes: FF D8 FF
        Assert.Equal(0xFF, result[0]);
        Assert.Equal(0xD8, result[1]);
        Assert.Equal(0xFF, result[2]);
    }

    // ── Guard / validation ────────────────────────────────────────────────────

    [Fact]
    public void CompressImage_WithNullBytes_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _processor.CompressImage(null!, 75));
    }

    [Fact]
    public void CompressImage_WithEmptyBytes_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _processor.CompressImage(Array.Empty<byte>(), 75));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(200)]
    public void CompressImage_WithQualityOutOfRange_ShouldThrowArgumentOutOfRangeException(
        int quality)
    {
        byte[] source = TestImageFactory.CreateJpeg();
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            _processor.CompressImage(source, quality));
    }
}
