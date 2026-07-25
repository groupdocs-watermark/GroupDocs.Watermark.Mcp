# Claude Code

```bash
claude mcp add groupdocs-watermark -- dnx GroupDocs.Watermark.Mcp --yes
```

With storage folder and license:

```bash
claude mcp add groupdocs-watermark -e GROUPDOCS_MCP_STORAGE_PATH=/path/to/documents -e GROUPDOCS_LICENSE_PATH=/path/to/GroupDocs.Total.lic -- dnx GroupDocs.Watermark.Mcp --yes
```

Pin a version by replacing `GroupDocs.Watermark.Mcp` with `GroupDocs.Watermark.Mcp@26.7.2`.
