using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Hexagrams.Nuke.Components;
using Nuke.Common.IO;

// ReSharper disable RedundantExtendsListEntry
// ReSharper disable InconsistentNaming

[DotNetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild,
    IHasGitRepository,
    IHasVersioning,
    IClean,
    IRestore,
    IFormat,
    ICompile,
    IPack,
    IPush
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

    [Solution]
    readonly Solution Solution;
    Solution IHasSolution.Solution => Solution;

    public IEnumerable<AbsolutePath> ExcludedFormatPaths => Enumerable.Empty<AbsolutePath>();

    Target ICompile.Compile => _ => _
        .Inherit<ICompile>()
        .DependsOn<IFormat>(x => x.VerifyFormat);

    Configure<DotNetPublishSettings> ICompile.PublishSettings => _ => _
        .When(!ScheduledTargets.Contains(((IPush)this).Push), _ => _
            .ClearProperties());

    Target IPush.Push => _ => _
        .Inherit<IPush>()
        .Consumes(this.FromComponent<IPush>().Pack)
        .Requires(() => this.FromComponent<IHasGitRepository>().GitRepository.Tags.Any())
        .WhenSkipped(DependencyBehavior.Execute);
}
