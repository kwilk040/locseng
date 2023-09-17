using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace Indexer;

public class Indexer
{
    private static readonly Logger Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
    private Index _index;

    private Indexer(Index index)
    {
        _index = index;
    }

    public void AddDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            Logger.Error($"Directory {dirPath} does not exist.");
            return;
        }

        Logger.Information($"Adding {dirPath} to index.");

        var filesPaths = Directory
            .EnumerateFiles(dirPath, "*", SearchOption.AllDirectories) //TODO: CATCH EXCEPTIONS
            .Where(f => !f.StartsWith("."))
            .Where(IsSupported)
            .ToList();

        foreach (var file in filesPaths)
        {
            if (!File.Exists(file)) continue;

            var lastWriteTime = File.GetLastWriteTime(file);
            if (!_index.RequiresReindexing(file, lastWriteTime))
            {
                Logger.Information($"File {file} does not require indexing.");
                continue;
            }

            var content = File.ReadAllText(file);
            Logger.Information($"Indexing => {file}, Last Write Time => {lastWriteTime}.");
            _index.AddDocument(file, lastWriteTime, content);
        }

        Task.Run(SerializeIndexToJson);
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

    public static Indexer Initialize()
    {
        var index = LoadIndexFromJson();
        return new Indexer(index);
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

    private bool IsSupported(string filename)
    {
        var extension = filename[filename.LastIndexOf(".", StringComparison.Ordinal)..];

        if (Enum.GetValues<SupportedExtension>().ToList()
            .ConvertAll(ext => new string(ext.StringValue())).Contains(extension))
        {
            return true;
        }

        Logger.Warning($"Can't index {filename}: Unsupported extension => {extension}");
        return false;
    }
}