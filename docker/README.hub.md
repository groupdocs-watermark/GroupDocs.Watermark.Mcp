# GroupDocs.Watermark MCP Server

MCP server that exposes [GroupDocs.Watermark](https://products.groupdocs.com/watermark) as AI-callable tools for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Quick start

```bash
docker run --rm -i \
  -v $(pwd)/documents:/data \
  groupdocs/watermark-net-mcp:latest
```

## Use with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-watermark": {
      "command": "docker",
      "args": ["run", "--rm", "-i", "-v", "/path/to/documents:/data", "groupdocs/watermark-net-mcp:latest"]
    }
  }
}
```

## Tools

- **AddWatermark** — Adds a text watermark (Arial, configurable size + rotation) to a document and saves the watermarked file to storage
- **AddImageWatermark** — Adds an image watermark (logo, signature scan, stamp) to a document and saves the watermarked file. Configurable opacity + rotation
- **SearchWatermarks** — Finds existing watermarks (text + image) in a document and returns their details (type, text, page, position, size, rotation) as JSON
- **RemoveWatermarks** — Removes existing watermarks from a document and saves the cleaned copy. Optional text filter to remove only matching watermarks
- **GetDocumentInfo** — Returns the file type, page count, and per-page dimensions of a document as JSON — without modifying it

## Tags & environment

- Tags: `latest` + an immutable version tag per release matching NuGet (e.g. `26.7.1`).
  Platforms: `linux/amd64`, `linux/arm64`. Also on GHCR: `ghcr.io/groupdocs-watermark/watermark-net-mcp`.
- `GROUPDOCS_MCP_STORAGE_PATH` (default `/data`), `GROUPDOCS_MCP_OUTPUT_PATH` (optional),
  `GROUPDOCS_LICENSE_PATH` — mount your license and point at it to leave evaluation mode
  (see the Licensing section in the GitHub README for the exact evaluation limits).

Full docs, one-click installs for other clients, and licensing details:
**https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp**
