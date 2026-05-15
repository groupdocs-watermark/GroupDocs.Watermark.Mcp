using System.ComponentModel;
using System.Text;
using System.Text.Json;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Options;
using ModelContextProtocol.Server;

namespace GroupDocs.Watermark.Mcp.Tools;

[McpServerToolType]
public static class GetDocumentInfoTool
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    [McpServerTool, Description(
        "Returns the file type, page count, and format-specific properties of a document as JSON. " +
        "Supports PDF, DOCX, XLSX, PPTX, PNG, JPG, and 50+ more document and image formats. " +
        "Call this tool whenever the user asks about the structure of a document — page count, dimensions, file type — without modifying it. " +
        "Useful as a precondition check before AddWatermark / SearchWatermarks (e.g. 'how many pages does this PDF have?'). " +
        "Do NOT pre-check whether the file exists — just pass the filename the user provided. " +
        "Returns a JSON object with fields `fileType` (engine-reported format name), `fileFormat` (extension), `size` (bytes), `pageCount`, and `pages` (array of `{ number, width, height }` per page). " +
        "On failure, the response text starts with 'Document-info lookup failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> GetDocumentInfo(
        IFileResolver resolver,
        ILicenseManager licenseManager,
        FileInput file,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        var ext = Path.GetExtension(resolved.FileName);
        var tempInput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{ext}");

        try
        {
            await using (var fs = File.Create(tempInput))
                await resolved.Stream.CopyToAsync(fs);

            var loadOptions = password != null ? new LoadOptions(password) : null;
            using var watermarker = loadOptions != null
                ? new Watermarker(tempInput, loadOptions)
                : new Watermarker(tempInput);

            var info = watermarker.GetDocumentInfo();

            // Serialize against the runtime type so subtype-specific properties
            // surface (per Step 5d guidance in the clone prompt — IDocumentInfo
            // is an interface; blind interface serialization drops subtype
            // fields like Pages, PageCount, etc.).
            var payload = new
            {
                fileName = resolved.FileName,
                fileType = info.FileType?.FileFormatName,
                fileFormat = info.FileType?.Extension,
                size = info.Size,
                pageCount = TryGetPageCount(info),
                pages = TryGetPages(info),
            };

            return JsonSerializer.Serialize(payload, JsonOptions);
        }
        catch (Exception ex)
        {
            return FormatException(ex, resolved.FileName);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
        }
    }

    /// IDocumentInfo's subtypes expose page count via `Pages.Count` or a typed
    /// `PageCount` property depending on format (PDF vs Office vs image).
    /// Reflection probe avoids per-subtype switch and gracefully returns null
    /// when neither shape is present (e.g. for single-frame images).
    private static int? TryGetPageCount(object info)
    {
        var pageCount = info.GetType().GetProperty("PageCount")?.GetValue(info);
        if (pageCount is int n) return n;

        var pages = info.GetType().GetProperty("Pages")?.GetValue(info);
        if (pages is System.Collections.ICollection coll) return coll.Count;

        return null;
    }

    private static object[]? TryGetPages(object info)
    {
        var pagesProp = info.GetType().GetProperty("Pages")?.GetValue(info);
        if (pagesProp is not System.Collections.IEnumerable pages) return null;

        var list = new List<object>();
        int idx = 1;
        foreach (var p in pages)
        {
            var width = p.GetType().GetProperty("Width")?.GetValue(p);
            var height = p.GetType().GetProperty("Height")?.GetValue(p);
            list.Add(new { number = idx, width, height });
            idx++;
        }
        return list.ToArray();
    }

    private static string FormatException(Exception ex, string fileName)
    {
        var sb = new StringBuilder();
        sb.Append($"Document-info lookup failed for '{fileName}': ");
        sb.Append($"{ex.GetType().FullName}: {ex.Message}");
        var inner = ex.InnerException;
        for (int depth = 0; inner != null && depth < 5; depth++, inner = inner.InnerException)
            sb.Append($" | inner({depth}): {inner.GetType().FullName}: {inner.Message}");
        return sb.ToString();
    }
}
