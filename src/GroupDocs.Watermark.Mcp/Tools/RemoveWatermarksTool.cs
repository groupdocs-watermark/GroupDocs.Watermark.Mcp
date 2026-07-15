using System.ComponentModel;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Options;
using ModelContextProtocol.Server;

namespace GroupDocs.Watermark.Mcp.Tools;

[McpServerToolType]
public static class RemoveWatermarksTool
{
    [McpServerTool, Description(
        "Removes existing watermarks from a document and saves the cleaned copy as '<name>_unwatermarked.<ext>'. " +
        "Supports PDF, DOCX, XLSX, PPTX, PNG, JPG, and 50+ more document and image formats. " +
        "Call this tool whenever the user asks to remove / strip / clean / delete watermarks from a document. " +
        "If `textFilter` is supplied, only watermarks whose text contains that substring (case-insensitive) are removed; otherwise ALL watermarks are removed. " +
        "Do NOT pre-check whether the file exists — just pass the filename the user provided. " +
        "Returns a saved-path message ('Removed <N> watermark(s) from \"<file>\"') and the download URL or storage path. " +
        "If no matching watermarks are found, the original document is saved unchanged with a message starting 'No matching watermarks found in'. " +
        "On failure, the response text starts with 'Watermark removal failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> RemoveWatermarks(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        FileInput file,
        [Description("Optional case-insensitive substring filter — only watermarks whose text contains this string are removed. Omit to remove ALL watermarks.")] string? textFilter = null,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        var ext = Path.GetExtension(resolved.FileName);
        var outputName = $"{Path.GetFileNameWithoutExtension(resolved.FileName)}_unwatermarked{ext}";
        var tempInput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{ext}");
        var tempOutput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{ext}");

        try
        {
            await using (var fs = File.Create(tempInput))
                await resolved.Stream.CopyToAsync(fs);

            var loadOptions = password != null ? new LoadOptions(password) : null;
            using var watermarker = loadOptions != null
                ? new Watermarker(tempInput, loadOptions)
                : new Watermarker(tempInput);

            var found = watermarker.Search();
            var toRemove = found
                .Where(w => string.IsNullOrEmpty(textFilter)
                    || (w.Text != null && w.Text.Contains(textFilter, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (toRemove.Count == 0)
            {
                return $"No matching watermarks found in '{resolved.FileName}'" +
                    (string.IsNullOrEmpty(textFilter) ? "." : $" (filter: '{textFilter}').");
            }

            foreach (var w in toRemove)
                watermarker.Remove(w);

            watermarker.Save(tempOutput);

            var bytes = await File.ReadAllBytesAsync(tempOutput);
            var savedPath = await storage.WriteFileAsync(outputName, bytes, rewrite: false);

            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include an evaluation watermark.\n\n";
            var filterSuffix = string.IsNullOrEmpty(textFilter) ? string.Empty : $" matching '{textFilter}'";
            return $"{prefix}Removed {toRemove.Count} watermark(s){filterSuffix} from '{resolved.FileName}'. Saved cleaned copy to '{savedPath}'.";
        }
        catch (Exception ex)
        {
            var suffix = string.IsNullOrEmpty(textFilter) ? null : $"(filter: '{textFilter}')";
            return ToolError.Format("Watermark removal", resolved.FileName, ex, suffix);
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }
}
