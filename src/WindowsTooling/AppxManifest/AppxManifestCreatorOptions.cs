namespace WindowsTooling.AppxManifest;

public class AppxManifestCreatorOptions
{
    public string[]? EntryPoints { get; set; }

    public AppxPackageArchitecture PackageArchitecture { get; set; }

    public string? PackageName { get; set; }

    public string? PackageDisplayName { get; set; }

    public string? PackageDescription { get; set; }

    public string? PublisherName { get; set; }

    public string? PublisherDisplayName { get; set; }

    public Version? Version { get; set; }

    public bool CreateLogo { get; set; }

    public static AppxManifestCreatorOptions Default =>
        new()
        {
            PackageArchitecture = AppxPackageArchitecture.Neutral,
            Version = null,
            PackageName = "MyPackage",
            PackageDisplayName = "My package",
            PublisherName = "CN=Publisher",
            PublisherDisplayName = "Publisher",
            CreateLogo = true
        };
}