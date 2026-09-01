using Calabonga.Commandex.Engine.Base;
using Calabonga.Commandex.Engine.Exceptions;
using Calabonga.Commandex.Engine.NugetDependencies;
using Calabonga.Commandex.Engine.Settings;
using Calabonga.Commandex.Shell.Extensions;
using Calabonga.OperationResults;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Serilog;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace Calabonga.Commandex.Shell.Engine;

/// <summary>
/// Nuget wrapper from current application
/// </summary>
public sealed class NugetLoader
{
    private readonly IAppSettings _appSettings;

    public NugetLoader(IAppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    /// <summary>
    /// Loads dependent nuget-packages from nuget feed.
    /// </summary>
    /// <param name="items"></param>
    /// <param name="sourceType"></param>
    /// <param name="artifactsFolderPath"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    public async Task<OperationEmpty<ExecuteCommandexCommandException>> LoadPackagesFromNugetAsync(ICommandexCommand command, List<INugetDependency> items, NuGetSourceType sourceType, string artifactsFolderPath, CancellationToken cancellationToken)
    {
        foreach (var dependency in items.SelectMany(x => x.Dependencies))
        {
            var packageId = dependency.Name;
            var version = dependency.Version;

            var extractedFiles = await LoadPackageByIdFromNugetAsync(command, packageId, version, sourceType, artifactsFolderPath, cancellationToken);

            if (!extractedFiles.Ok)
            {
                App.Current.LastException = extractedFiles.Error;
                return Operation.Error(extractedFiles.Error);
            }

            foreach (var file in extractedFiles.Result)
            {
                var extension = Path.GetExtension(file);
                if (extension == ".dll")
                {
                    Assembly.LoadFrom(file);
                }
            }
        }

        return Operation.Result();
    }

    /// <summary>
    /// Returns a nuget-repository depend on <see cref="NuGetSourceType"/>
    /// </summary>
    /// <param name="sourceType">local artifacts feed or the remote feed configured via NUGET_FEED_URL</param>
    /// <param name="localFeedPath">path used as the package source when <paramref name="sourceType"/> is Local</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private SourceRepository GetRepository(NuGetSourceType sourceType, string localFeedPath)
    {
        return sourceType switch
        {
            NuGetSourceType.Local => Repository.Factory.GetCoreV3(new PackageSource(localFeedPath, "AppDefinitionsRepository", true, false, false)),
            NuGetSourceType.Remote => Repository.Factory.GetCoreV3(_appSettings.NugetFeedUrl),
            _ => throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null)
        };
    }

    /// <summary>
    /// Loads from nuget package local storage (artifacts)
    /// </summary>
    /// <param name="command"></param>
    /// <param name="packageId"></param>
    /// <param name="version"></param>
    /// <param name="sourceType"></param>
    /// <param name="artifactsFolderPath"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The list of loaded assemblies</returns>
    /// <exception cref="Exception"></exception>
    private async Task<Operation<List<string>, ExecuteCommandexCommandException>> LoadPackageByIdFromNugetAsync(
        ICommandexCommand command,
        string packageId,
        string version,
        NuGetSourceType sourceType,
        string artifactsFolderPath,
        CancellationToken cancellationToken)
    {
        var globalNuget = Path.Combine(artifactsFolderPath, "packages");
        var repository = GetRepository(sourceType, globalNuget);
        var downloadResource = await repository.GetResourceAsync<DownloadResource>(cancellationToken);
        if (!NuGetVersion.TryParse(version, out var nuGetVersion))
        {
            var downloadException = new ExecuteCommandexCommandException($"Invalid version {version} for nuget package {packageId} ({sourceType})");
            return Operation.Error(downloadException);
        }

        using var downloadResourceResult = await downloadResource.GetDownloadResourceResultAsync(
            new PackageIdentity(packageId, nuGetVersion),
            new PackageDownloadContext(new SourceCacheContext()),
            globalPackagesFolder: globalNuget,
            logger: NullLogger.Instance,
            token: cancellationToken);

        if (downloadResourceResult.Status != DownloadResourceResultStatus.Available)
        {
            var statusException = new ExecuteCommandexCommandException($"DownloadAsync of NuGet package failed. DownloadResult Status: {downloadResourceResult.Status}");
            return Operation.Error(statusException);
        }

        var reader = downloadResourceResult.PackageReader;

        var archive = new ZipArchive(downloadResourceResult.PackageStream);

        var libItems = await reader.GetLibItemsAsync(cancellationToken);

        var all = libItems.ToList();

        if (all.Count > 1)
        {
            all = all.FindCompatible();
        }

        var loadedDllPaths = new List<string>();
        var definitionArtifactFolder = Path.Combine(artifactsFolderPath, command.TypeName);
        foreach (var libItem in all)
        {
            var files = libItem.Items.Where(x => x.EndsWith(".dll"))
                .Where(x => !x.Contains("resources"));

            foreach (var libItemItem in files)
            {
                var entry = archive.GetEntry(libItemItem);
                using var memoryStream = new MemoryStream();
                await entry!.Open().CopyToAsync(memoryStream, cancellationToken);
                var assemblyLoadContext = new System.Runtime.Loader.AssemblyLoadContext(null, isCollectible: true);
                memoryStream.Position = 0;
                assemblyLoadContext.LoadFromStream(memoryStream);
                var dllFile = Path.Combine(definitionArtifactFolder!, entry!.Name);
                entry.SaveAsFile(dllFile, NullLogger.Instance);
                Log.Debug(dllFile);
                loadedDllPaths.Add(dllFile);
            }
        }

        return Operation.Result(loadedDllPaths);
    }
}
