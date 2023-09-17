using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace Indexer;

public class Indexer
{
    private readonly Logger _logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
    private readonly Index _index = new();

    public void AddDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath))
        {
            _logger.Error($"Directory {dirPath} does not exist.");
            return;
        }

        _logger.Information($"Adding {dirPath} to index.");

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
                _logger.Information($"File {file} does not require indexing.");
                continue;
            }

            var content = File.ReadAllText(file);
            _logger.Information($"Indexing => {file}, Last Write Time => {lastWriteTime}.");
            _index.AddDocument(file, lastWriteTime, content);
        }

        Task.Run(SerializeIndexToJson);
    }

    public List<KeyValuePair<string, double>> QueryIndex(string query)
    {
        _logger.Information($"QUERY => {query}");
        return _index.Search(query);
    }

    private void SerializeIndexToJson()
    {
        const string filename = "index.json";
        _logger.Information($"Saving index to {filename}");
        var json = JsonConvert.SerializeObject(_index);
        File.WriteAllText(filename, json);
        _logger.Information("Index saved");
    }

    private bool IsSupported(string filename)
    {
        var extension = filename[filename.LastIndexOf(".", StringComparison.Ordinal)..];

        if (Enum.GetValues<SupportedExtension>().ToList()
            .ConvertAll(ext => new string(ext.StringValue())).Contains(extension))
        {
            return true;
        }

        _logger.Warning($"Can't index {filename}: Unsupported extension => {extension}");
        return false;
    }
}