# JetBrains Rider (2025.2+)

Settings -> Tools -> AI Assistant -> Model Context Protocol (MCP) -> Add. Choose
**As JSON** and paste:

```json
{
  "name": "groupdocs-watermark",
  "command": "dnx",
  "args": ["GroupDocs.Watermark.Mcp", "--yes"],
  "env": {
    "GROUPDOCS_MCP_STORAGE_PATH": "/path/to/documents",
    "GROUPDOCS_LICENSE_PATH": "/path/to/GroupDocs.Total.lic"
  }
}
```

Remove `GROUPDOCS_LICENSE_PATH` to run in evaluation mode. Pin a version by
replacing `GroupDocs.Watermark.Mcp` with `GroupDocs.Watermark.Mcp@26.7.1`.
