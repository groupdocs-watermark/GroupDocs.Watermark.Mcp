using System.ComponentModel;
using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Common;
using GroupDocs.Watermark.Options;
using GroupDocs.Watermark.Watermarks;
using ModelContextProtocol.Server;

namespace GroupDocs.Watermark.Mcp.Tools;

[McpServerToolType]
public static class AddImageWatermarkTool
{
    [McpServerTool, Description(
        "Adds an image watermark (e.g. company logo, signature scan, stamp) to a document and saves the watermarked file. " +
        "Supports PDF, DOCX, XLSX, PPTX, PNG, JPG, and 50+ more document and image formats as the target. " +
        "The watermark source image can be PNG, JPG, BMP, or TIFF, resolved from the same storage as the target. " +
        "Call this tool whenever the user asks to add an image / logo / stamp watermark, or to overlay an image on a document. " +
        "Do NOT pre-check whether files exist — just pass the filenames the user provided. " +
        "Returns a saved-path message ('Added image watermark from \"<image>\" to \"<file>\"') and the download URL or storage path. " +
        "On failure, the response text starts with 'Image watermarking failed for' followed by the underlying exception type, message, and inner-exception chain.")]
    public static async Task<string> AddImageWatermark(
        IFileResolver resolver,
        IFileStorage storage,
        ILicenseManager licenseManager,
        FileInput file,
        [Description("Image file to use as the watermark — resolved from the same storage as `file`.")] FileInput watermarkImage,
        [Description("Opacity 0.0 (fully transparent) to 1.0 (opaque). Default 0.5.")] double opacity = 0.5,
        [Description("Rotation angle in degrees (default 0)")] int rotation = 0,
        [Description("Password for protected target documents")] string? password = null)
    {
        licenseManager.SetLicense();
        using var resolved = await resolver.ResolveAsync(file);
        using var resolvedImage = await resolver.ResolveAsync(watermarkImage);

        var ext = Path.GetExtension(resolved.FileName);
        var imageExt = Path.GetExtension(resolvedImage.FileName);
        var outputName = $"{Path.GetFileNameWithoutExtension(resolved.FileName)}_watermarked{ext}";
        var tempInput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{ext}");
        var tempImage = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{imageExt}");
        var tempOutput = Path.Combine(Path.GetTempPath(), $"gd_mcp_{Guid.NewGuid()}{ext}");

        try
        {
            await using (var fs = File.Create(tempInput))
                await resolved.Stream.CopyToAsync(fs);
            await using (var fs = File.Create(tempImage))
                await resolvedImage.Stream.CopyToAsync(fs);

            var loadOptions = password != null ? new LoadOptions(password) : null;
            using var watermarker = loadOptions != null
                ? new Watermarker(tempInput, loadOptions)
                : new Watermarker(tempInput);

            var watermark = new ImageWatermark(tempImage)
            {
                Opacity = opacity,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                RotateAngle = rotation,
            };

            watermarker.Add(watermark);
            watermarker.Save(tempOutput);

            var bytes = await File.ReadAllBytesAsync(tempOutput);
            var savedPath = await storage.WriteFileAsync(outputName, bytes, rewrite: false);

            var prefix = licenseManager.IsLicensed ? string.Empty : "[Evaluation mode] Output may include evaluation watermarks alongside the user-requested watermark.\n\n";
            return $"{prefix}Added image watermark from '{resolvedImage.FileName}' to '{resolved.FileName}'. Saved to '{savedPath}'.";
        }
        catch (Exception ex)
        {
            return ToolError.Format("Image watermarking", resolved.FileName, ex, $"(image='{resolvedImage.FileName}')");
        }
        finally
        {
            if (File.Exists(tempInput)) File.Delete(tempInput);
            if (File.Exists(tempImage)) File.Delete(tempImage);
            if (File.Exists(tempOutput)) File.Delete(tempOutput);
        }
    }
}
