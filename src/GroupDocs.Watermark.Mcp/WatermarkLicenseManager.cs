using GroupDocs.Mcp.Core;
using GroupDocs.Mcp.Core.Licensing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GroupDocs.Watermark.Mcp;

public class WatermarkLicenseManager : LicenseManager
{
    public WatermarkLicenseManager(IOptions<McpConfig> config, ILogger<LicenseManager> logger)
        : base(config, logger)
    {
    }

    // Identifies the engine in get_license_status. Without it the tool would report the
    // server's own version, because this class lives in the server assembly.
    protected override Type? EngineMarkerType => typeof(GroupDocs.Watermark.License);

    protected override void SetLicenseFromPath(string licensePath)
    {
        new GroupDocs.Watermark.License().SetLicense(licensePath);
    }

    protected override void SetMeteredKeyCore(string publicKey, string privateKey)
    {
        new GroupDocs.Watermark.Metered().SetMeteredKey(publicKey, privateKey);
    }

    protected override MeteredConsumption ReadConsumptionCore()
    {
        // Static on the engine and only meaningful once a metered key is applied - Core
        // guarantees this runs in metered mode only.
        return new MeteredConsumption
        {
            Quantity = GroupDocs.Watermark.Metered.GetConsumptionQuantity(),
            Credit = GroupDocs.Watermark.Metered.GetConsumptionCredit()
        };
    }
}
