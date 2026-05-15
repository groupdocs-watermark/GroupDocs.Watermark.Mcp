using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Mcp.Tools;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GroupDocs.Watermark.Mcp.Tests;

public class AddWatermarkToolTests
{
    private readonly Mock<IFileResolver> _resolver = new();
    private readonly Mock<ILicenseManager> _licenseManager = new();
    private readonly Mock<IFileStorage> _storage = new();
    private readonly OutputHelper _output;

    public AddWatermarkToolTests()
    {
        _output = new OutputHelper(_storage.Object, Microsoft.Extensions.Options.Options.Create(new McpConfig()));
    }

    [Fact]
    public async Task AddWatermark_WhenResolverThrows_PropagatesException()
    {
        // Resolver runs BEFORE the try/catch in AddWatermarkTool, so its
        // exceptions propagate (file-not-found is a caller error, not an
        // engine error worth wrapping).
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("missing.pdf"));

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            AddWatermarkTool.AddWatermark(
                _resolver.Object,
                _storage.Object,
                _licenseManager.Object,
                _output,
                new FileInput { FilePath = "missing.pdf" },
                text: "DRAFT"));

        Assert.Contains("missing.pdf", ex.Message);
    }

    [Fact]
    public async Task AddWatermark_SetsLicense_BeforeResolving()
    {
        var sequence = new List<string>();

        _licenseManager
            .Setup(l => l.SetLicense())
            .Callback(() => sequence.Add("license"));

        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .Callback(() => sequence.Add("resolve"))
            .ThrowsAsync(new InvalidOperationException("short-circuit"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AddWatermarkTool.AddWatermark(
                _resolver.Object,
                _storage.Object,
                _licenseManager.Object,
                _output,
                new FileInput { FilePath = "anything.pdf" },
                text: "DRAFT"));

        Assert.Equal(new[] { "license", "resolve" }, sequence);
    }

    [Fact]
    public async Task AddWatermark_PassesFileInputToResolver_Unchanged()
    {
        var input = new FileInput { FilePath = "report.pdf" };
        FileInput? captured = null;

        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .Callback<FileInput, CancellationToken>((fi, _) => captured = fi)
            .ThrowsAsync(new InvalidOperationException("short-circuit"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AddWatermarkTool.AddWatermark(
                _resolver.Object,
                _storage.Object,
                _licenseManager.Object,
                _output,
                input,
                text: "DRAFT"));

        Assert.Same(input, captured);
    }
}
