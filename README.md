# GroupDocs.Watermark MCP Server

MCP server that exposes [GroupDocs.Watermark](https://products.groupdocs.com/watermark) as AI-callable tools
for Claude, Cursor, GitHub Copilot, and other MCP agents.

## Installation

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

**Run directly with `dnx` (recommended — no install step):**

```bash
dnx GroupDocs.Watermark.Mcp --yes
```

Pulls the latest stable release on every invocation. To pin to a specific
version (recommended for shared configs and CI), append `@<version>`:

```bash
dnx GroupDocs.Watermark.Mcp@25.6.0 --yes
```

**Or install as a global dotnet tool:**

```bash
dotnet tool install -g GroupDocs.Watermark.Mcp
groupdocs-watermark-mcp
```

**Or run via Docker:**

```bash
docker run --rm -i \
  -v $(pwd)/documents:/data \
  ghcr.io/groupdocs-watermark/watermark-net-mcp:latest
```

## Available MCP Tools

| Tool | Description |
|---|---|
| `AddWatermark` | Adds a text watermark (Arial, configurable size + rotation) to a document and saves the watermarked file to storage |
| `AddImageWatermark` | Adds an image watermark (logo, signature scan, stamp) to a document and saves the watermarked file. Configurable opacity + rotation |
| `SearchWatermarks` | Finds existing watermarks (text + image) in a document and returns their details (type, text, page, position, size, rotation) as JSON |
| `RemoveWatermarks` | Removes existing watermarks from a document and saves the cleaned copy. Optional text filter to remove only matching watermarks |
| `GetDocumentInfo` | Returns the file type, page count, and per-page dimensions of a document as JSON — without modifying it |

All tools support PDF, DOCX, XLSX, PPTX, PNG, JPG, VSDX, and 50+ more document and image formats.

## Example prompts for AI agents

Copy any of these into Claude Desktop, Cursor, or GitHub Copilot Chat after the
server is connected. The AI agent will pick the right tool and arguments
automatically — file names are resolved against `GROUPDOCS_MCP_STORAGE_PATH`.

1. **Add a DRAFT text watermark**: *"Add a watermark with the text 'DRAFT' to report.pdf."*
2. **Stamp a logo image**: *"Use company-logo.png as an image watermark on every page of contract.docx, opacity 30%."*
3. **Inspect existing watermarks**: *"What watermarks does invoice.pdf contain?"*
4. **Strip old watermarks**: *"Remove all watermarks from final-draft.docx so I can ship it."*
5. **Inspect document structure**: *"How many pages does presentation.pptx have, and what are their dimensions?"*

## Configuration

| Variable | Description | Default |
|---|---|---|
| `GROUPDOCS_MCP_STORAGE_PATH` | Base folder for input and output files | current directory |
| `GROUPDOCS_MCP_OUTPUT_PATH` | *(Optional)* separate folder for output files | `GROUPDOCS_MCP_STORAGE_PATH` |
| `GROUPDOCS_LICENSE_PATH` | Path to GroupDocs license file. In evaluation mode, output documents may include an additional evaluation watermark alongside the user-requested one | (evaluation mode) |

## Usage with Claude Desktop

```json
{
  "mcpServers": {
    "groupdocs-watermark": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Watermark.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "/path/to/documents"
      }
    }
  }
}
```

> To pin to a specific version, replace `"GroupDocs.Watermark.Mcp"` with
> `"GroupDocs.Watermark.Mcp@25.6.0"` in `args`. Pinning is recommended for
> shared / committed configs to avoid surprise upgrades.

## Usage with VS Code / GitHub Copilot

NuGet.org generates a ready-to-use `mcp.json` snippet on the [package page](https://www.nuget.org/packages/GroupDocs.Watermark.Mcp).
Copy it directly into your `.vscode/mcp.json`.

Alternatively, add manually to `.vscode/mcp.json`:

```json
{
  "inputs": [
    {
      "type": "promptString",
      "id": "storage_path",
      "description": "Base folder for input and output files.",
      "password": false
    }
  ],
  "servers": {
    "groupdocs-watermark": {
      "type": "stdio",
      "command": "dnx",
      "args": ["GroupDocs.Watermark.Mcp", "--yes"],
      "env": {
        "GROUPDOCS_MCP_STORAGE_PATH": "${input:storage_path}"
      }
    }
  }
}
```

> Same pinning rule as above — swap `"GroupDocs.Watermark.Mcp"` for
> `"GroupDocs.Watermark.Mcp@25.6.0"` to lock to a specific release.

## Usage with Docker Compose

```bash
cd docker
docker compose up
```

Edit `docker/docker-compose.yml` to point volumes at your local documents folder.

## Documentation & guides

Step-by-step deployment guides and a published-package integration test suite
live in the companion repo
[**GroupDocs.Watermark.Mcp.Tests**](https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp.Tests):

- [Install from NuGet](https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp.Tests/blob/master/how-to/01-install-from-nuget.md) — `dnx`, global tool, pinned vs always-latest
- [Run via Docker](https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp.Tests/blob/master/how-to/02-run-via-docker.md)
- [Verify on the MCP registry](https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp.Tests/blob/master/how-to/03-verify-mcp-registry.md)
- [Use with Claude Desktop](https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp.Tests/blob/master/how-to/04-use-with-claude-desktop.md)
- [Use with VS Code / GitHub Copilot](https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp.Tests/blob/master/how-to/05-use-with-vscode-copilot.md)
- [Run the integration tests](https://github.com/groupdocs-watermark/GroupDocs.Watermark.Mcp.Tests/blob/master/how-to/06-run-integration-tests.md)

That repo also exercises every advertised tool against the **published** NuGet
artifact on Linux, macOS, and Windows in CI — so the snippets above are
verified end-to-end on every release.

## License

MIT — see [LICENSE](LICENSE)

<!-- mcp-name: io.github.groupdocs-watermark/groupdocs-watermark-mcp -->
