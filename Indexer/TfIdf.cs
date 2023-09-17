namespace Indexer;

internal static class TfIdf
{
    internal static double ComputeTfIdf(string token, Document doc, int docCount,
        IReadOnlyDictionary<string, int> documentFrequency)
    {
        return ComputeTf(token, doc) * ComputeIdf(token, docCount, documentFrequency);
    }

    private static double ComputeTf(string token, Document doc)
    {
        var n = doc.Count;
        var m = doc.TermFrequency.TryGetValue(token, out var value) ? value : 0;
        return 1.0 * m / n;
    }

    private static double ComputeIdf(string token, int n, IReadOnlyDictionary<string, int> documentFrequency)
    {
        var m = documentFrequency.TryGetValue(token, out var value) ? value : 1;
        return Math.Log10(1.0 * n / m);
    }
}
