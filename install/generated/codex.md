# Codex CLI (OpenAI)

```bash
codex mcp add groupdocs-watermark -- dnx GroupDocs.Watermark.Mcp --yes
```

Or add to `~/.codex/config.toml`:

```toml
[mcp_servers.groupdocs-watermark]
command = "dnx"
args = ["GroupDocs.Watermark.Mcp", "--yes"]

[mcp_servers.groupdocs-watermark.env]
GROUPDOCS_MCP_STORAGE_PATH = "/path/to/documents"
GROUPDOCS_MCP_OUTPUT_PATH = "/path/to/documents"
GROUPDOCS_LICENSE_PATH = ""   # empty = evaluation mode; set to your GroupDocs.Total.lic to lift limits
```

Pin a version by replacing `GroupDocs.Watermark.Mcp` with `GroupDocs.Watermark.Mcp@26.9.0`.
