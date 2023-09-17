using Newtonsoft.Json;

namespace Indexer;

internal class Index
{
    [JsonProperty] private readonly Dictionary<string, Document> _documents = new();
    [JsonProperty] private readonly Dictionary<string, int> _documentFrequency = new();

    internal List<KeyValuePair<string, double>> Search(string query)
    {
        var result = new Dictionary<string, double>();
        var queryTokens = new Tokenizer(new List<char>(query)).ToList();

        foreach (var pathDocument in _documents)
        {
            var rank = queryTokens.Sum(token =>
                TfIdf.ComputeTfIdf(token, pathDocument.Value, _documents.Count, _documentFrequency));
            if (rank is double.NaN or 0) continue;
            result.Add(pathDocument.Key, rank);
        }

        var sorted = from entry in result
            orderby entry.Value descending
            select entry;
        return sorted.ToList();
    }

    private void RemoveDocument(string filePath)
    {
        if (!_documents.ContainsKey(filePath)) return;
        foreach (var tf in _documents[filePath].TermFrequency
                     .Where(tf => _documentFrequency
                         .ContainsKey(tf.Key)))
        {
            _documentFrequency[tf.Key] -= 1;
        }

        _documents.Remove(filePath);
    }

    internal bool RequiresReindexing(string filePath, DateTime lastModified)
    {
        if (_documents.TryGetValue(filePath, out var document))
        {
            return document.LastModified < lastModified;
        }

        return true;
    }

    internal void AddDocument(string filePath, DateTime lastModified, string content)
    {
        RemoveDocument(filePath);
        var termFrequency = new Dictionary<string, int>();
        var count = 0;
        foreach (var token in new Tokenizer(new List<char>(content)))
        {
            if (termFrequency.ContainsKey(token))
            {
                termFrequency[token] += 1;
            }
            else
            {
                termFrequency.Add(token, 1);
            }

            count++;
        }

        foreach (var token in termFrequency.Keys)
        {
            if (_documentFrequency.ContainsKey(token))
            {
                _documentFrequency[token] += 1;
            }
            else
            {
                _documentFrequency.Add(token, 1);
            }
        }

        _documents.Add(filePath, new Document(termFrequency, count, lastModified));
    }
}