using ImageProcessingEngine.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using Xunit;

namespace ImageProcessingEngine.Tests;

/// <summary>
/// TDD tests for <see cref="ImageProcessor.ResizeImage"/>.
/// Tests were written BEFORE the implementation (Red → Green → Refactor).
/// </summary>
public class ResizeTests
{
    private readonly ImageProcessor _processor = new(NullLogger.Instance);

    // ── Happy-path ────────────────────────────────────────────────────────────

    [Fact]
    public void ResizeImage_JPEG_ShouldReturnImageWithExactDimensions()
    {
        byte[] source = TestImageFactory.CreateJpeg(200, 150);

        byte[] result = _processor.ResizeImage(source, 100, 80);

        using var img = Image.Load(result);
        Assert.Equal(100, img.Width);
        Assert.Equal(80,  img.Height);
    }

    [Fact]
    public void ResizeImage_PNG_ShouldReturnImageWithExactDimensions()
    {
        byte[] source = TestImageFactory.CreatePng(200, 150);

        byte[] result = _processor.ResizeImage(source, 50, 50);

        using var img = Image.Load(result);
        Assert.Equal(50, img.Width);
        Assert.Equal(50, img.Height);
    }

    [Fact]
    public void ResizeImage_ShouldReturnNonEmptyByteArray()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.ResizeImage(source, 100, 75);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void ResizeImage_OutputShouldBeDecodable()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.ResizeImage(source, 64, 64);

        // Image.Load should not throw — guarantees the bytes are a valid image.
        var exception = Record.Exception(() => Image.Load(result));
        Assert.Null(exception);
    }

    // ── Guard / validation ────────────────────────────────────────────────────

    [Fact]
    public void ResizeImage_WithNullBytes_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _processor.ResizeImage(null!, 100, 100));
    }

    [Fact]
    public void ResizeImage_WithEmptyBytes_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _processor.ResizeImage(Array.Empty<byte>(), 100, 100));
    }

    [Theory]
    [InlineData(0,   100)]
    [InlineData(-1,  100)]
    [InlineData(100, 0)]
    [InlineData(100, -5)]
    [InlineData(0,   0)]
    public void ResizeImage_WithInvalidDimensions_ShouldThrowArgumentException(int width, int height)
    {
        byte[] source = TestImageFactory.CreateJpeg();
        Assert.Throws<ArgumentException>(() => _processor.ResizeImage(source, width, height));
    }
}
