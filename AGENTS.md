# AGENTS.md — Guide for AI coding agents

Brief orientation for AI coding agents (Claude Code, Copilot, Cursor, Aider, Amp, Codex) working in this repository.

## What this repo is

A standalone **MCP server** for [GroupDocs.Watermark for .NET](https://products.groupdocs.com/watermark) — exposes document-watermark operations as AI-callable tools via the Model Context Protocol.

Published to NuGet as `GroupDocs.Watermark.Mcp` with the `McpServer` package type, and to `ghcr.io/groupdocs-watermark/watermark-net-mcp` + `docker.io/groupdocs/watermark-net-mcp` as a container image.

## MCP tools exposed

| Tool | Description |
|---|---|
| `AddWatermark` | Adds a text watermark (Arial, configurable size + rotation) to a document and writes the watermarked copy to storage. Returns a saved-path message + download URL. |
| `AddImageWatermark` | Adds an image watermark (logo, signature scan, stamp) to a document. Configurable opacity + rotation. Returns a saved-path message + download URL. |
| `SearchWatermarks` | Finds existing watermarks (both text and image) in a document and returns their details (type, text, page, position, size, rotation) as JSON. |
| `RemoveWatermarks` | Removes existing watermarks from a document and saves the cleaned copy as `<name>_unwatermarked.<ext>`. Optional `textFilter` to remove only watermarks whose text contains the filter substring. |
| `GetDocumentInfo` | Returns the file type, page count, size, and per-page dimensions of a document as JSON. Does not modify the document. |

All tools accept `FileInput` (resolved via `IFileResolver`) and an optional `password` for protected documents. All tools wrap engine-level exceptions in a descriptive error string (each tool uses a per-operation prefix: `Watermarking failed for`, `Image watermarking failed for`, `Search failed for`, `Watermark removal failed for`, `Document-info lookup failed for`) followed by the exception type, message, and inner-exception chain — never letting them bubble into MCP's generic `An error occurred invoking '<tool>'` wrapper. Critical for diagnosing native-deps issues on Linux.

## Folder layout

```
src/                                           ← all projects + sln + Directory.Build.props
  GroupDocs.Watermark.Mcp/
    Program.cs                                 ← host bootstrap + stdio transport
    WatermarkLicenseManager.cs                 ← applies GroupDocs.Total license
    Tools/
      AddWatermarkTool.cs                      ← [McpServerTool] — AddWatermark
      AddImageWatermarkTool.cs                 ← [McpServerTool] — AddImageWatermark
      SearchWatermarksTool.cs                  ← [McpServerTool] — SearchWatermarks
      RemoveWatermarksTool.cs                  ← [McpServerTool] — RemoveWatermarks
      GetDocumentInfoTool.cs                   ← [McpServerTool] — GetDocumentInfo
    .mcp/
      server.json                              ← NuGet.org reads this to generate mcp.json snippet
    GroupDocs.Watermark.Mcp.csproj             ← PackageType=McpServer + ToolCommandName
  GroupDocs.Watermark.Mcp.Tests/
  GroupDocs.Watermark.Mcp.sln
  Directory.Build.props
build/
  dependencies.props                           ← single source of truth for all versions
changelog/                                     ← one MD file per change (see changelog/README.md)
docker/
  Dockerfile                                   ← multi-stage, runtime on aspnet:10.0
  docker-compose.yml
.github/workflows/                             ← build_packages.yml, run_tests.yml, publish_prod.yml, publish_docker.yml
```

## Dependencies

- `GroupDocs.Mcp.Core` + `GroupDocs.Mcp.Local.Storage` — infrastructure NuGet packages from the [GroupDocs.Mcp.Core](https://github.com/groupdocs/GroupDocs.Mcp.Core) repo
- `GroupDocs.Watermark` — the actual watermarking engine (uses System.Drawing.Font / Color in PDF + Diagram content paths, hence the `System.Drawing.EnableUnixSupport=true` runtime flag in the csproj and `libgdiplus` + `libfontconfig1` + `ttf-mscorefonts-installer` in the Dockerfile)
- `ModelContextProtocol` — MCP SDK for .NET
- `Microsoft.Extensions.Hosting` — host builder for the stdio server
- `SkiaSharp.NativeAssets.Linux.NoDependencies` — pinned (the upstream `GroupDocs.Watermark` nuspec declares it, but pinning explicitly keeps transitive resolution deterministic on Linux)

## Commands you can run

```bash
# Restore + build
dotnet restore
dotnet build src/GroupDocs.Watermark.Mcp.sln -c Release

# Run tests
dotnet test src/GroupDocs.Watermark.Mcp.sln -c Release

# Run the server locally (stdio)
dotnet run --project src/GroupDocs.Watermark.Mcp

# Local pack (writes to ./build_out) — validates server.json version matches dependencies.props
pwsh ./build.ps1

# Build + run the Docker image
docker build -f docker/Dockerfile -t watermark-net-mcp:local .
docker run --rm -i -v $(pwd)/documents:/data watermark-net-mcp:local
```

## Version scheme

CalVer `YY.MM.N`. The version lives in **two** places that MUST stay in lockstep:
1. `build/dependencies.props` → `<GroupDocsWatermarkMcp>`
2. `src/GroupDocs.Watermark.Mcp/.mcp/server.json` → both top-level `"version"` and `packages[0].version`

`build.ps1` enforces this at pack time (`Assert-ServerJsonVersionMatchesDependencies`) — if they drift, the build fails.

## House rules

1. **Tools must have rich `[Description("...")]` strings** — these are what AI agents read via the MCP protocol. Write them as task-oriented sentences, not method-signature summaries. Enumerate supported formats and describe the response format (JSON shape or saved-path text) so AI clients can post-process correctly.
2. **Never add new env vars beyond** `GROUPDOCS_MCP_STORAGE_PATH`, `GROUPDOCS_MCP_OUTPUT_PATH`, `GROUPDOCS_LICENSE_PATH` without updating `server.json`, `docker-compose.yml`, and `README.md` together.
3. **JSON-emitting tools return raw JSON** — do not pass through `OutputHelper.TruncateText`. Its truncation marker is plain text and breaks `JsonDocument.Parse`. See `SearchWatermarksTool` for the pattern.
4. **Engine calls live inside a `try/catch`** that returns a descriptive `<Operation> failed for '<file>': <ExceptionType>: <message>` string. Keep `resolver.ResolveAsync` OUTSIDE the catch so file-not-found errors propagate cleanly.
5. **Tests use xUnit + Moq** — mock `IFileResolver`, `IFileStorage`, `ILicenseManager`, `OutputHelper`.
6. **Changelog entries required** — any PR that changes behaviour adds `changelog/NNN-slug.md`.
7. **Do not edit `obj/` or `build_out/`** — build artifacts.
8. **Target framework is `net10.0` only** — required by `dnx` and the MCP SDK.

## Release flow

See [RELEASE.md](RELEASE.md) for the exact per-release checklist.

## What NOT to change

- Do not hardcode the version in `.csproj` — it flows from `$(GroupDocsWatermarkMcp)` in `dependencies.props`.
- Do not remove the `<PackageType>McpServer</PackageType>` or `<ToolCommandName>groupdocs-watermark-mcp</ToolCommandName>` from the csproj — NuGet.org discoverability and `dnx` invocation depend on them.
- Do not remove the `StripNativeRuntimePdbs` MSBuild target — it keeps the nupkg under NuGet.org's 250 MB hard limit by deleting Windows-native SkiaSharp PDBs from the publish output.
- Do not change the `.mcp/server.json` schema URL without cross-checking with the NuGet MCP docs.
