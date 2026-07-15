using System.ComponentModel;
using System.Text.Json;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Options;
using ModelContextProtocol.Server;

namespace GroupDocs.Watermark.Mcp.Tools;

[McpServerToolType]
public static class SearchWatermarksTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description(
        "Searches for watermarks in a document and returns their details as JSON. " +
        "Supports PDF, DOCX, XLSX, PPTX, PNG, JPG, and 50+ more document and image formats. " +
        "Call this tool immediately whenever the user asks to search for watermarks, find watermarks, list watermarks, or check if a document has watermarks. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "Returns a JSON object with fields `count` (number of watermarks found) and `watermarks` (array with `type` (\"text\"|\"image\"), `text`, `page`, `x`, `y`, `width`, `height`, `rotateAngle` per watermark). " +
        "On failure, the response text starts with 'Search failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> SearchWatermarks(
        IFileResolver resolver,
        ILicenseManager licenseManager,
        FileInput file,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        var tempInput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{Path.GetExtension(resolved.FileName)}");
        try
        {
            await using (var fs = File.Create(tempInput))
                await resolved.Stream.CopyToAsync(fs);

            var loadOptions = password != null ? new LoadOptions(password) : null;
            using var watermarker = loadOptions != null
                ? new Watermarker(tempInput, loadOptions)
                : new Watermarker(tempInput);

            var watermarks = watermarker.Search();

            var results = watermarks.Select(w => new
            {
                type = w.ImageData != null ? "image" : "text",
                text = w.Text,
                page = w.PageNumber,
                x = w.X,
                y = w.Y,
                width = w.Width,
                height = w.Height,
                rotateAngle = w.RotateAngle
            }).ToList();

            // Return raw JSON directly. Do NOT pipe through OutputHelper.TruncateText —
            // its truncation marker is plain text and breaks strict-JSON consumers
            // when the result exceeds McpConfig.MaxOutputCharacters (default 5000).
            // See Pitfall #16 in the clone-to-new-product.md prompt.
            return JsonSerializer.Serialize(new { count = results.Count, watermarks = results }, JsonOptions);
        }
        catch (Exception ex)
        {
            return ToolError.Format("Search", resolved.FileName, ex);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
        }
    }
}
