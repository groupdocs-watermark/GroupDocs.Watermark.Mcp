using System.ComponentModel;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Common;
using GroupDocs.Watermark.Options;
using GroupDocs.Watermark.Watermarks;
using ModelContextProtocol.Server;

namespace GroupDocs.Watermark.Mcp.Tools;

[McpServerToolType]
public static class AddWatermarkTool
{
    [McpServerTool, Description(
        "Adds a text watermark to a document and saves the watermarked file to storage. " +
        "Supports PDF, DOCX, XLSX, PPTX, PNG, JPG, and 50+ more document and image formats. " +
        "Call this tool immediately whenever the user asks to add a watermark, stamp text onto a document, or mark a document as draft/confidential. " +
        "Do NOT pre-check whether files exist — just pass the filename the user provided. " +
        "Returns a saved-path message ('Added text watermark \"<text>\" to \"<file>\"') and the download URL or storage path. " +
        "On failure, the response text starts with 'Watermarking failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> AddWatermark(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        OutputHelper output,
        FileInput file,
        [Description("Watermark text to add")] string text,
        [Description("Font size (default 36)")] int fontSize = 36,
        [Description("Rotation angle in degrees (default -45)")] int rotation = -45,
        [Description("Password for protected documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);

        var ext = Path.GetExtension(resolved.FileName);
        var outputName = $"{Path.GetFileNameWithoutExtension(resolved.FileName)}_watermarked{ext}";
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

            var watermark = new TextWatermark(text, new Font("Arial", fontSize))
            {
                ForegroundColor = Color.FromArgb(128, 192, 192, 192),
                Opacity = 0.5,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                RotateAngle = rotation
            };

            watermarker.Add(watermark);
            watermarker.Save(tempOutput);

            var bytes = await File.ReadAllBytesAsync(tempOutput);
            var savedPath = await storage.WriteFileAsync(outputName, bytes, rewrite: false);

            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include evaluation watermarks alongside the user-requested watermark.\n\n";
            return await output.BuildFileOutputAsync(savedPath, $"{prefix}Added text watermark '{text}' to '{resolved.FileName}'");
        }
        catch (Exception ex)
        {
            // Surface the underlying engine exception (type + message + inner
            // chain) instead of letting it bubble up to ModelContextProtocol's
            // generic "An error occurred invoking 'add_watermark'." wrapper.
            // Pattern lifted from Conversion 26.5.2 — diagnostics for native-deps
            // issues on Linux (missing fonts, libgdiplus) without requiring local
            // reproduction.
            return ToolError.Format("Watermarking", resolved.FileName, ex, $"(text='{text}')");
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }
}
