namespace Helper;

internal static class VersionHelper
{
    internal static int Major { get; set; }

    internal static int Minor { get; set; }

    internal static int Build { get; set; }

    internal static int RevisionOrPreviewNumber { get; set; }

    internal static bool IsPreview { get; set; }

    internal static string GetVersionString(
        bool allowPreviewSyntax,
        bool excludeRevisionOrPreviewNumber)
    {
        if (excludeRevisionOrPreviewNumber)
        {
            return $"{Major}.{Minor}.{Build}";
        }

        if (allowPreviewSyntax)
        {
            return $"{Major}.{Minor}.{Build}-pre.{RevisionOrPreviewNumber}";
        }

        return $"{Major}.{Minor}.{Build}.{RevisionOrPreviewNumber}";
    }
}
