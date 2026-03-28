# Changelog

All notable changes to the Image Processing Engine project will be documented in this file.

## [1.0.0] - 2026-03-28

### Released
- **Date:** March 28, 2026, 01:37 PM
- **Revision:** 1

### Added
- **Initial Release: Image Processing Engine v1.0.0**
- **ResizeImage:** Scale images to exact pixel dimensions.
- **CropImage:** Extract rectangular regions from images with built-in boundary validation.
- **ConvertImageFormat:** Seamlessly convert between popular formats including JPEG, PNG, and WebP.
- **CompressImage:** Reduce file size using controlled JPEG compression levels (1-100).
- **GenerateThumbnail:** Intelligent aspect-ratio-preserving scaling based on a maximum side length.

### Technical Highlights
- **Managed Performance:** Built using SixLabors.ImageSharp (100% managed C#), ensuring perfect compatibility with ODC’s Linux container runtime without native dependency overhead.
- **Cloud Observability:** Fully integrated with ODC's logging system (ILogger) and distributed tracing (Activity Source) for deep monitoring in the Portal.
- **Memory Efficient:** Operations are performed entirely in-memory using Streams to handle large image processing tasks without temporary file overhead.
- **TDD Verified:** Validated by a comprehensive suite of 60 automated unit tests covering edge cases, invalid inputs, and format signatures.

### Usage Note
- All actions accept and return **BinaryData**, allowing direct integration with database attributes and file upload widgets.
