using Serilog;
using Serilog.Core;

namespace indexer;

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
            .Where(f => !f.StartsWith(".")) // filter out hidden files
            .Where(f => f.EndsWith(".md")) //TODO: MORE EXTENSIONS
            .ToList();

        foreach (var file in filesPaths)
        {
            if (!File.Exists(file)) continue;
            // TODO: determine if extension is supported

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

        // TODO: save index
    }

    public List<KeyValuePair<string, double>> QueryIndex(string query)
    {
        _logger.Information($"QUERY => {query}");
        return _index.Search(query);
    }
}
