using System.Collections;
using Serilog;
using Serilog.Core;

namespace Indexer;

/// <summary>
///  Exception free Directory.EnumerateFiles replacement.
/// <see keyword="Source" href="https://stackoverflow.com/questions/13130052/directoryinfo-enumeratefiles-causes-unauthorizedaccessexception-and-other"/>
/// </summary>
public class SafeDirectory : IEnumerable<FileSystemInfo>
{
    private static readonly Logger Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();

    private readonly DirectoryInfo _root;
    private readonly IList<string> _patterns;
    private readonly SearchOption _option;

    public static IEnumerable<FileSystemInfo> EnumerateFiles(string root, string pattern,
        SearchOption option = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"The folder '{root}' could not be located.");

        var rootDirectoryInfo = new DirectoryInfo(root);
        return new SafeDirectory(rootDirectoryInfo, pattern, option);
    }

    private SafeDirectory(DirectoryInfo root, string pattern, SearchOption option)
    {
        _root = root;
        _patterns = new List<string> { pattern };
        _option = option;
    }

    private SafeDirectory(DirectoryInfo root, IList<string> patterns, SearchOption option)
    {
        _root = root;
        _patterns = patterns;
        _option = option;
    }

    public IEnumerator<FileSystemInfo> GetEnumerator()
    {
        if (!_root.Exists) yield break;

        IEnumerable<FileSystemInfo> matches = new List<FileSystemInfo>();
        try
        {
            matches = _patterns.Aggregate(matches, (current, pattern) => current
                .Concat(_root.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly)));
        }
        catch (UnauthorizedAccessException)
        {
            Logger.Warning($"Unable to access {_root.FullName} ");
            yield break;
        }
        catch (IOException)
        {
            Logger.Warning($"Could not process path {_root.FullName}");
            yield break;
        }


        foreach (var file in matches)
        {
            yield return file;
        }

        if (_option != SearchOption.AllDirectories)
            yield break;

        foreach (var dir in _root.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var fileSystemInfos = new SafeDirectory(dir, _patterns, _option);
            foreach (var match in fileSystemInfos)
            {
                yield return match;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}