---
id: 001
date: 2026-05-14
version: 26.5.0
type: feature
---

# Initial public release of GroupDocs.Watermark MCP Server

## What changed
- NuGet package `GroupDocs.Watermark.Mcp` published with `McpServer` package type.
- Five MCP tools exposed:
  - `AddWatermark` — add a text watermark (Arial, configurable font size + rotation) to a document and save the watermarked copy to storage.
  - `AddImageWatermark` — add an image watermark (logo, signature scan, stamp) to a document. Configurable opacity + rotation. Returns a saved-path message and download URL.
  - `SearchWatermarks` — find existing watermarks (text and image) in a document and return their type, text, page, position, size, and rotation as JSON.
  - `RemoveWatermarks` — remove existing watermarks from a document and save the cleaned copy as `<name>_unwatermarked.<ext>`. Optional `textFilter` argument scopes removal to watermarks whose text contains a given substring.
  - `GetDocumentInfo` — return the file type, page count, size, and per-page dimensions of a document as JSON (without modifying it). Useful as a precondition check before AddWatermark / SearchWatermarks.
- Installable via `dnx GroupDocs.Watermark.Mcp@26.5.0 --yes` (.NET 10 SDK required) or `dotnet tool install -g`.
- Docker image published to `ghcr.io/groupdocs-watermark/watermark-net-mcp` and `docker.io/groupdocs/watermark-net-mcp`.
- Environment variables: `GROUPDOCS_MCP_STORAGE_PATH`, optional `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH`.
- Linux native graphics deps wired up: `SkiaSharp.NativeAssets.Linux.NoDependencies` (3.119.0) is referenced because `GroupDocs.Watermark` 26.4.0 transitively requires SkiaSharp ≥ 3.119.0; `libgdiplus` + `libfontconfig1` + `ttf-mscorefonts-installer` are installed in the Docker image (and the Tests-repo integration workflow) because Watermark's PDF and Diagram content paths use `System.Drawing.Font` / `Color` / `FontFamily` / `FontStyle`; the `System.Drawing.EnableUnixSupport` runtime flag is set in the csproj for the same reason.

## Pre-shipped pitfall remediations
- **JSON-returning tools never pipe through `OutputHelper.TruncateText`** — `SearchWatermarksTool` returns raw `JsonSerializer.Serialize(...)` directly. The framework subproject's original implementation routed through `TruncateText`, which would have appended a non-JSON marker on responses > 5 KB and broken strict-JSON consumers. Fixed at clone time.
- **Engine exceptions surface diagnostically** — both `AddWatermarkTool` and `SearchWatermarksTool` wrap their engine calls in `try/catch (Exception ex)` and return `"Watermarking failed for '<file>': <ExceptionType>: <message> | inner(0): ..."` (or `"Search failed for '<file>': ..."`) instead of letting them bubble up to `ModelContextProtocol`'s default handler, which replaces all exception detail with a canned `"An error occurred invoking 'add_watermark'"` string. Pattern lifted from Conversion 26.5.2 — makes native-deps regressions on Linux diagnosable from the first failing CI run.
- **License class is exposed publicly** — `public sealed class License` in `groupdocs-watermark-net/src/GroupDocs.Watermark/License.cs:38` is exposed with a `public void SetLicense(string filePath)` method. `WatermarkLicenseManager` uses the Metadata pattern (`new GroupDocs.Watermark.License().SetLicense(licensePath)`) rather than an environment-variable fallback.

## Why
Fourth product MCP server in the GroupDocs MCP framework (after Metadata, Conversion, Comparison, Viewer). Exposes GroupDocs.Watermark for .NET as AI-callable tools for Claude, Cursor, VS Code / GitHub Copilot, and other MCP-compatible agents.

## Migration / impact
First release — no migration required.
