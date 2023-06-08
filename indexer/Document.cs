namespace indexer;

internal readonly record struct Document(Dictionary<string, int> TermFrequency, int Count, DateTime LastModified);
