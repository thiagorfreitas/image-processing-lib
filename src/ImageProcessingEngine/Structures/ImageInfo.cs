using OutSystems.ExternalLibraries.SDK;

namespace ImageProcessingEngine.Structures;

/// <summary>
/// Metadata about an image.  Decorated with [OSStructure] so it can be used
/// as a return type or parameter in future OSInterface methods.
/// (Currently not returned by the initial 5 actions — reserved for a GetImageInfo action.)
/// </summary>
[OSStructure(Description = "Metadata describing properties of an image.")]
public struct ImageInfo
{
    [OSStructureField(Description = "Width of the image in pixels.")]
    public int Width { get; set; }

    [OSStructureField(Description = "Height of the image in pixels.")]
    public int Height { get; set; }

    [OSStructureField(Description = "Detected format of the image (e.g. 'jpeg', 'png', 'webp').")]
    public string Format { get; set; }

    [OSStructureField(Description = "Size of the image data in bytes.")]
    public int FileSizeBytes { get; set; }
}
