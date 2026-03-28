# ImageProcessingEngine — ODC External Library

A production-ready **OutSystems Developer Cloud (ODC) External Library** written in C# .NET 8 that exposes server-side image manipulation operations as **Server Actions** in ODC apps.

---

## Exposed Server Actions (ODC)

| Server Action | Description |
|---|---|
| `ResizeImage` | Resize to exact pixel dimensions |
| `CropImage` | Crop a rectangular region |
| `ConvertImageFormat` | Convert to `jpeg`, `png`, or `webp` |
| `CompressImage` | Re-encode as JPEG at a given quality (1–100) |
| `GenerateThumbnail` | Scale to fit within a square of N×N px (aspect ratio preserved) |

All actions accept and return `byte[]` — which maps to OutSystems **BinaryData** type.

---

## Project Structure

```
ImageProcessingEngine/
├── ImageProcessingEngine.sln
├── src/
│   └── ImageProcessingEngine/
│       ├── IImageProcessor.cs         ← [OSInterface] — ODC Server Action definitions
│       ├── ImageProcessor.cs          ← Implementation using SixLabors.ImageSharp
│       ├── Guard.cs                   ← Defensive validation helpers
│       └── Structures/
│           └── ImageInfo.cs           ← [OSStructure] — reserved for GetImageInfo (v2)
├── tests/
│   └── ImageProcessingEngine.Tests/
│       ├── Fixtures/TestImageFactory.cs
│       ├── ResizeTests.cs             ← 8 tests
│       ├── CropTests.cs               ← 9 tests
│       ├── FormatConversionTests.cs   ← 11 tests
│       ├── CompressionTests.cs        ← 9 tests
│       └── ThumbnailTests.cs          ← 9 tests  (46 total)
└── scripts/
    └── generate_upload_package.ps1    ← test → publish → zip
```

---

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- PowerShell 7+ (for the packaging script)

---

## Running Tests

```bash
dotnet test tests/ImageProcessingEngine.Tests
```

All 46 tests must pass before packaging.

---

## Build & Package for ODC

```powershell
.\scripts\generate_upload_package.ps1
```

This script:
1. Runs all unit tests (fails fast on failure)
2. Publishes for `linux-x64` (the ODC container runtime)
3. Creates `ImageProcessingEngine.zip` in the repo root

---

## Upload to ODC Portal

1. Go to **ODC Portal → Extend your apps → External Libraries**
2. Click **Upload** and select `ImageProcessingEngine.zip`
3. ODC validates the package and introspects the `[OSInterface]`
4. Create an **External Library**, publish, and release it
5. Consume the 5 Server Actions in any ODC app

---

## Design Decisions

| Decision | Rationale |
|---|---|
| **SixLabors.ImageSharp** | 100% managed C# — no native binaries. Safe for ODC linux-x64 containers. SkiaSharp requires native `.so` which adds deployment friction. |
| `byte[]` I/O | Maps directly to OutSystems `BinaryData`. No serialization overhead. |
| `NullLogger` / `ILogger` injection | ODC injects a logger at runtime; logs surface in the ODC Portal log viewer. |
| `Activity.Current?.Source.StartActivity` | Distributed tracing — spans appear in ODC's trace viewer. |
| `Guard` helper class | Centralised input validation keeps action methods clean and readable. |
| JPEG output for Compress & Thumbnail | JPEG is the dominant lossy format for size reduction use cases. |
| Format-preserving Resize & Crop | PNG→PNG, JPEG→JPEG — no lossy re-encoding unless explicitly requested. |

---

## Licensing Note

SixLabors ImageSharp uses the **Six Labors Split License**.  
- Free for open-source projects  
- A commercial license is required for production commercial use  

See: https://sixlabors.com/pricing/

---

## Example Usage (in ODC Server Action flow logic)

```
# In an ODC Server Action using the ExternalLibrary:

1. Receive binary image from a client upload (BinaryData)
2. Call ImageProcessor.ResizeImage(Image: BinaryData, Width: 800, Height: 600)
3. Call ImageProcessor.CompressImage(Image: ResizedImage, Quality: 80)
4. Store the result or return it to the client
```
