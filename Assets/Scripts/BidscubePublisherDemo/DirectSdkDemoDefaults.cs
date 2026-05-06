/// <summary>
/// Direct SDK smoke-test defaults when <see cref="BidscubeDemoConfig.json"/> omits placements or uses YOUR_* placeholders.
/// Same sample ID as <c>BidscubeSDK.BasicIntegration.AdExample</c> in the core UPM package.
/// </summary>
public static class DirectSdkDemoDefaults
{
    public const string PlacementIdFallbackAllFormats = "test_placement";

    public static string ResolvePlacement(string fromConfig)
    {
        if (BidscubeDemoConfigLoader.LooksLikeDemoPlaceholder(fromConfig))
            return PlacementIdFallbackAllFormats;
        return fromConfig.Trim();
    }
}
