using Markdig;
using NUglify;

namespace Indexer;

internal static class Parser
{
    internal static string GetContent(FileSystemInfo file)
    {
        if (!Enum.TryParse(file.Extension.Replace(".", ""), true, out SupportedExtension extension))
            throw new ArgumentException($"Cannot parse {file.FullName}: Extension not supported {file.Extension}");

        var path = file.FullName;
        return extension switch
        {
            SupportedExtension.Txt => ParseTxt(path),
            SupportedExtension.Md => ParseMd(path),
            SupportedExtension.Html => ParseHtml(path),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static string ParseTxt(string path)
    {
        return File.ReadAllText(path);
    }

    private static string ParseMd(string path)
    {
        var content = File.ReadAllText(path);
        return Markdown.ToPlainText(content);
    }

    private static string ParseHtml(string path)
    {
        var html = File.ReadAllText(path);
        var result = Uglify.HtmlToText(html).Code!;
        return result;
    }
}