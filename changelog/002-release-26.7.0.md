---
id: 002
date: 2026-07-15
version: 26.7.0
type: maintenance
---

# Release 26.7.0 — engine bump to 26.6.0 + shared error helper

## What changed
- Bumped the MCP server version `26.5.0` → `26.7.0` (CalVer).
- Bumped the underlying engine `GroupDocs.Watermark` `26.4.0` → `26.6.0`.
- **Removed the manual `SkiaSharp.NativeAssets.Linux.NoDependencies` package
  reference** (and the `<SkiaSharp>` version property). `GroupDocs.Watermark`
  26.6.0 now ships a dedicated `net10.0` runtime sub-package
  (`GroupDocs.Watermark.Net100`) which — resolved under `net10.0` — declares
  `SkiaSharp` 3.119.0 + `SkiaSharp.NativeAssets.Linux.NoDependencies`
  transitively. The native `libSkiaSharp.so` continues to ship in the nupkg via
  transitive resolution, verified in the packed `runtimes/` tree. (Pitfall S.)
- **Extracted a shared `Tools/ToolError.cs`** (`ToolError.Format(op, file, ex,
  subjectSuffix)`) and routed all five tools' `catch` blocks through it, replacing
  five duplicated private `FormatException` helpers. The user-visible failure text
  is byte-for-byte unchanged — every tool still emits its per-tool prefix
  (`Watermarking failed for`, `Image watermarking failed for`, `Search failed for`,
  `Watermark removal failed for`, `Document-info lookup failed for`).

## Why
Routine dependency refresh onto the latest stable engine, plus the cross-product
convention of a single descriptive-error formatter (mirrors GroupDocs.Metadata
26.7.x). The 26.6.0 TFM split lets the MCP drop its manual Skia workaround.

## Migration / impact
No API or behaviour change. Tool names, schemas, response shapes, and error
prefixes are identical. Consumers pinning `@26.5.0` should move to `@26.7.0`.
