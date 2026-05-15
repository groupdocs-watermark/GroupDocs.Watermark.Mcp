using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Mcp.Tools;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GroupDocs.Watermark.Mcp.Tests;

public class SearchWatermarksToolTests
{
    private readonly Mock<IFileResolver> _resolver = new();
    private readonly Mock<ILicenseManager> _licenseManager = new();
    private readonly Mock<IFileStorage> _storage = new();
    private readonly OutputHelper _output;

    public SearchWatermarksToolTests()
    {
        _output = new OutputHelper(_storage.Object, Microsoft.Extensions.Options.Options.Create(new McpConfig()));
    }

    [Fact]
    public async Task SearchWatermarks_WhenResolverThrows_PropagatesException()
    {
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("missing.pdf"));

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            SearchWatermarksTool.SearchWatermarks(
                _resolver.Object,
                _licenseManager.Object,
                new FileInput { FilePath = "missing.pdf" }));

        Assert.Contains("missing.pdf", ex.Message);
    }

    [Fact]
    public async Task SearchWatermarks_SetsLicense_BeforeResolving()
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
            SearchWatermarksTool.SearchWatermarks(
                _resolver.Object,
                _licenseManager.Object,
                new FileInput { FilePath = "anything.pdf" }));

        Assert.Equal(new[] { "license", "resolve" }, sequence);
    }

    [Fact]
    public async Task SearchWatermarks_PassesFileInputToResolver_Unchanged()
    {
        var input = new FileInput { FilePath = "report.pdf" };
        FileInput? captured = null;

        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .Callback<FileInput, CancellationToken>((fi, _) => captured = fi)
            .ThrowsAsync(new InvalidOperationException("short-circuit"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SearchWatermarksTool.SearchWatermarks(
                _resolver.Object,
                _licenseManager.Object,
                input));

        Assert.Same(input, captured);
    }
}
