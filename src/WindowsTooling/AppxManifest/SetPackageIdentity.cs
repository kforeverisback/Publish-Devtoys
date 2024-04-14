﻿namespace WindowsTooling.AppxManifest;

public class SetPackageIdentity : IAppxEditCommand
{
    public string? Publisher { get; set; }

    public string? Version { get; set; }

    public string? Name { get; set; }

    public string? ProcessorArchitecture { get; set; }

    public string? ResourceId { get; set; }
}
