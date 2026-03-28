using ImageProcessingEngine.Tests.Fixtures;
using Microsoft.Extensions.Logging.Abstractions;
using SixLabors.ImageSharp;
using Xunit;

namespace ImageProcessingEngine.Tests;

/// <summary>
/// TDD tests for <see cref="ImageProcessor.GenerateThumbnail"/>.
///
/// Key rule: after thumbnailing, Max(width, height) must be &lt;= maxSize.
/// Aspect ratio must be preserved:
///   - A landscape image (width > height) → width == maxSize
///   - A portrait image (height > width)  → height == maxSize
///   - A square image                     → width == height == maxSize
/// </summary>
public class ThumbnailTests
{
    private readonly ImageProcessor _processor = new(NullLogger.Instance);

    // ── Happy-path ────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateThumbnail_LandscapeImage_LongestSideShouldEqualMaxSize()
    {
        // Landscape: 400×200
        byte[] source = TestImageFactory.CreateJpeg(width: 400, height: 200);

        byte[] result = _processor.GenerateThumbnail(source, maxSize: 100);

        using var img = Image.Load(result);
        Assert.Equal(100, img.Width);         // width (longest side) == maxSize
        Assert.True(img.Height <= 100,
            $"Height ({img.Height}) should be <= maxSize (100).");
    }

    [Fact]
    public void GenerateThumbnail_PortraitImage_LongestSideShouldEqualMaxSize()
    {
        // Portrait: 200×400
        byte[] source = TestImageFactory.CreateJpeg(width: 200, height: 400);

        byte[] result = _processor.GenerateThumbnail(source, maxSize: 100);

        using var img = Image.Load(result);
        Assert.Equal(100, img.Height);        // height (longest side) == maxSize
        Assert.True(img.Width <= 100,
            $"Width ({img.Width}) should be <= maxSize (100).");
    }

    [Fact]
    public void GenerateThumbnail_SquareImage_BothSidesShouldEqualMaxSize()
    {
        byte[] source = TestImageFactory.CreateJpeg(width: 300, height: 300);

        byte[] result = _processor.GenerateThumbnail(source, maxSize: 100);

        using var img = Image.Load(result);
        Assert.Equal(100, img.Width);
        Assert.Equal(100, img.Height);
    }

    [Fact]
    public void GenerateThumbnail_MaxSizeLargerThanImage_ShouldNotUpscale()
    {
        // Source is 50×50; maxSize is 200 — image should NOT be enlarged.
        byte[] source = TestImageFactory.CreateJpeg(width: 50, height: 50);

        byte[] result = _processor.GenerateThumbnail(source, maxSize: 200);

        using var img = Image.Load(result);
        // ImageSharp ResizeMode.Max will keep the original size when it already fits.
        Assert.True(img.Width  <= 200);
        Assert.True(img.Height <= 200);
    }

    [Fact]
    public void GenerateThumbnail_OutputShouldBeDecodable()
    {
        byte[] source = TestImageFactory.CreatePng();

        byte[] result = _processor.GenerateThumbnail(source, maxSize: 80);

        var exception = Record.Exception(() => Image.Load(result));
        Assert.Null(exception);
    }

    [Fact]
    public void GenerateThumbnail_OutputShouldBeNonEmpty()
    {
        byte[] source = TestImageFactory.CreateJpeg();

        byte[] result = _processor.GenerateThumbnail(source, maxSize: 64);

        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    // ── Guard / validation ────────────────────────────────────────────────────

    [Fact]
    public void GenerateThumbnail_WithNullBytes_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _processor.GenerateThumbnail(null!, 100));
    }

    [Fact]
    public void GenerateThumbnail_WithEmptyBytes_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            _processor.GenerateThumbnail(Array.Empty<byte>(), 100));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GenerateThumbnail_WithInvalidMaxSize_ShouldThrowArgumentException(int maxSize)
    {
        byte[] source = TestImageFactory.CreateJpeg();
        Assert.Throws<ArgumentException>(() =>
            _processor.GenerateThumbnail(source, maxSize));
    }
}
