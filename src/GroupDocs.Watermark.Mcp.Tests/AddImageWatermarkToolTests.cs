using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using GroupDocs.Watermark.Mcp.Tools;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace GroupDocs.Watermark.Mcp.Tests;

public class AddImageWatermarkToolTests
{
    private readonly Mock<IFileResolver> _resolver = new();
    private readonly Mock<ILicenseManager> _licenseManager = new();
    private readonly Mock<IFileStorage> _storage = new();

    [Fact]
    public async Task AddImageWatermark_WhenResolverThrows_PropagatesException()
    {
        _resolver
            .Setup(r => r.ResolveAsync(It.IsAny<FileInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("missing.pdf"));

        var ex = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            AddImageWatermarkTool.AddImageWatermark(
                _resolver.Object,
                _storage.Object,
                _licenseManager.Object,
                new FileInput { FilePath = "missing.pdf" },
                new FileInput { FilePath = "logo.png" }));

        Assert.Contains("missing.pdf", ex.Message);
    }

    [Fact]
    public async Task AddImageWatermark_SetsLicense_BeforeResolving()
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
            AddImageWatermarkTool.AddImageWatermark(
                _resolver.Object,
                _storage.Object,
                _licenseManager.Object,
                new FileInput { FilePath = "anything.pdf" },
                new FileInput { FilePath = "logo.png" }));

        Assert.Equal(new[] { "license", "resolve" }, sequence);
    }
}
