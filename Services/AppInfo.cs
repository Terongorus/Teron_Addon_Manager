using System.Reflection;

namespace TeronAddonManager.Services;

/// <summary>
/// Reads the app's display name from the assembly's &lt;Product&gt; metadata (set in
/// TeronAddonManager.csproj) instead of duplicating it as a separate hardcoded literal, so the
/// window title can't drift out of sync with the project file.
/// </summary>
internal static class AppInfo
{
    public static string DisplayName { get; } = GetDisplayName();

    private static string GetDisplayName()
    {
        string product = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product
            ?? "TeronAddonManager";
        int parenIndex = product.IndexOf(" (", StringComparison.Ordinal);
        return parenIndex > 0 ? product[..parenIndex] : product;
    }
}
