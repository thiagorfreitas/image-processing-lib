using ImageProcessingEngine.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using Xunit;

namespace ImageProcessingEngine.Tests;

/// <summary>
/// TDD tests for <see cref="ImageProcessor.CropImage"/>.
/// </summary>
public class CropTests
{
    private readonly ImageProcessor _processor = new(NullLogger.Instance);

    // ── Happy-path ────────────────────────────────────────────────────────────

    [Fact]
    public void CropImage_ShouldReturnImageWithExactDimensions()
    {
        // Source: 200×150. Crop a 80×60 region from (10, 10).
        byte[] source = TestImageFactory.CreateJpeg(200, 150);

        byte[] result = _processor.CropImage(source, x: 10, y: 10, width: 80, height: 60);

        using var img = Image.Load(result);
        Assert.Equal(80, img.Width);
        Assert.Equal(60, img.Height);
    }

    [Fact]
    public void CropImage_FromOrigin_ShouldReturnCorrectDimensions()
    {
        byte[] source = TestImageFactory.CreateJpeg(200, 150);

        byte[] result = _processor.CropImage(source, x: 0, y: 0, width: 100, height: 100);

        using var img = Image.Load(result);
        Assert.Equal(100, img.Width);
        Assert.Equal(100, img.Height);
    }

    [Fact]
    public void CropImage_OutputShouldBeDecodable()
    {
        byte[] source = TestImageFactory.CreatePng(200, 150);

        byte[] result = _processor.CropImage(source, 0, 0, 50, 50);

        var exception = Record.Exception(() => Image.Load(result));
        Assert.Null(exception);
    }

    // ── Guard / validation ────────────────────────────────────────────────────

    [Fact]
    public void CropImage_WithNullBytes_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _processor.CropImage(null!, 0, 0, 50, 50));
    }

    [Fact]
    public void CropImage_WithEmptyBytes_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _processor.CropImage(Array.Empty<byte>(), 0, 0, 50, 50));
    }

    [Theory]
    [InlineData(-1, 0,  50, 50)]  // negative x
    [InlineData(0, -1,  50, 50)]  // negative y
    [InlineData(0,  0,   0, 50)]  // zero width
    [InlineData(0,  0,  50,  0)]  // zero height
    [InlineData(0,  0,  -5, 50)]  // negative width
    [InlineData(0,  0,  50, -5)]  // negative height
    public void CropImage_WithInvalidParameters_ShouldThrowArgumentException(
        int x, int y, int width, int height)
    {
        byte[] source = TestImageFactory.CreateJpeg(200, 150);
        Assert.Throws<ArgumentException>(() =>
            _processor.CropImage(source, x, y, width, height));
    }

    [Fact]
    public void CropImage_WithRegionExceedingImageBounds_ShouldThrowArgumentException()
    {
        // Source is 200×150 — requesting a crop that overflows the right edge.
        byte[] source = TestImageFactory.CreateJpeg(200, 150);

        Assert.Throws<ArgumentException>(() =>
            _processor.CropImage(source, x: 150, y: 0, width: 100, height: 100));
    }
}
