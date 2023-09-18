using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace Indexer;

public class Indexer
{
    private static readonly Logger Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();

    private readonly Index _index;

    private Indexer(Index index)
    {
        _index = index;
    }

    public static Indexer Initialize()
    {
        var index = LoadIndexFromJson();
        return new Indexer(index);
    }

    public void AddDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Logger.Error($"Directory {dirPath} does not exist.");
            return;
        }

        Logger.Information($"Adding {dirPath} to index.");

        var filesPaths = GetPaths(dirPath);

        foreach (var file in filesPaths)
        {
            if (!file.Exists) continue;

            var path = file.FullName;
            var lastWriteTime = file.LastWriteTime;
            if (!_index.RequiresReindexing(path, lastWriteTime))
            {
                Logger.Information($"File {file} does not require indexing.");
                continue;
            }

            var content = File.ReadAllText(path);
            Logger.Information($"Indexing => {file}, Last Write Time => {lastWriteTime}.");
            _index.AddDocument(path, lastWriteTime, content);
        }

        _index.AddDirectory(dirPath);
        SerializeIndexToJson();
    }

    public void RemoveDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Logger.Error($"Directory {dirPath} does not exist.");
            return;
        }

        Logger.Information($"Removing {dirPath} from index.");

        var filesPaths = GetPaths(dirPath);

        foreach (var file in filesPaths.Where(f => f.Exists))
        {
            Logger.Information($"Removing => {file} from index");
            _index.RemoveDocument(file.FullName);
        }

        _index.RemoveDirectory(dirPath);
        SerializeIndexToJson();
    }

    private static IEnumerable<FileSystemInfo> GetPaths(string dirPath)
    {
        return SafeDirectory.EnumerateFiles(dirPath, "*")
            .Where(IsSupported)
            .ToList();
    }

    public List<KeyValuePair<string, double>> QueryIndex(string query)
    {
        Logger.Information($"QUERY => {query}");
        return _index.Search(query);
    }

    private void SerializeIndexToJson()
    {
        const string filename = "index.json";
        Logger.Information($"Saving index to {filename}");
        var json = JsonConvert.SerializeObject(_index);
        File.WriteAllText(filename, json);
        Logger.Information("Index saved");
    }


    private static Index LoadIndexFromJson()
    {
        string indexAsJson;
        try
        {
            indexAsJson = File.ReadAllText("index.json");
        }
        catch (FileNotFoundException)
        {
            Logger.Warning($"Could not read index.json");
            return new Index();
        }

        var index = JsonConvert.DeserializeObject<Index>(indexAsJson)!;
        Logger.Information("Loaded content from index.json");
        return index;
    }

    private static bool IsSupported(FileSystemInfo file)
    {
        var extension = file.Extension;

        if (Enum.GetValues<SupportedExtension>().ToList()
            .ConvertAll(ext => new string(ext.StringValue())).Contains(extension))
            return true;

        Logger.Warning($"Can't index {file}: Unsupported extension => {extension}");
        return false;
    }
}