using System.Collections;
using static System.Char;

namespace indexer;

internal class Tokenizer : IEnumerable<string>
{
    private readonly List<char> _content;

    internal Tokenizer(List<char> content)
    {
        _content = content;
    }

    private void TrimLeft()
    {
        while (_content.Any() && IsWhiteSpace(_content[0]))
        {
            _content.RemoveAt(0);
        }
    }

    private string NextToken()
    {
        TrimLeft();
        if (!_content.Any()) return "";
        if (IsDigit(_content[0])) return SliceWhile(IsDigit);
        return IsLetter(_content[0]) ? SliceWhile(IsLetter).ToUpper() : Slice(1);
    }

    private string SliceWhile(Predicate<char> condition)
    {
        var i = 0;
        while (i < _content.Count && condition.Invoke(_content[i])) i++;

        return Slice(i);
    }

    private string Slice(int i)
    {
        var tokenChars = _content.GetRange(0, i);
        var token = string.Concat(tokenChars);
        _content.RemoveRange(0, i);

        return token;
    }

    public IEnumerator<string> GetEnumerator()
    {
        while (_content.Any())
        {
            var token = NextToken();
            if (string.IsNullOrWhiteSpace(token)) continue;
            yield return token;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}