using Nuke.Common.IO;

namespace Core;

internal abstract class PublishBinariesBuilder
{
    protected PublishBinariesBuilder(string name, TargetCpuArchitecture architecture, bool selfContained)
    {
        Name = name;
        Architecture = architecture;
        SelfContained = selfContained;
    }

    internal string Name { get; }

    internal TargetCpuArchitecture Architecture { get; }

    internal bool SelfContained { get; }

    internal abstract void Build(AbsolutePath outputDirectory, Configuration configuration);
}
