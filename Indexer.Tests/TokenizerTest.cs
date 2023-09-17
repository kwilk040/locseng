namespace Indexer.Tests;

public class TokenizerTest
{
    [Fact]
    public void Tokenizer_InputIsLettersOnly_ReturnTokensWithUppercaseLetters()
    {
        const string input = "This is tokenizer test";
        var expectedTokens = new List<string> { "THIS", "IS", "TOKENIZER", "TEST" };
        var tokenizer = new Tokenizer(new List<char>(input));

        var tokens = tokenizer.ToList();

        Assert.Equal(expectedTokens, tokens);
    }

    [Fact]
    public void Tokenizer_InputContainsWhitespaces_ReturnTokensWithoutWhitespace()
    {
        const string input = "    This is tokenizer    test    ";
        var expectedTokens = new List<string> { "THIS", "IS", "TOKENIZER", "TEST" };
        var tokenizer = new Tokenizer(new List<char>(input));

        var tokens = tokenizer.ToList();

        Assert.Equal(expectedTokens, tokens);
    }

    [Fact]
    public void Tokenizer_InputContainsSpecialCharacters_ReturnTokensWithCharactersAsSeparateTokens()
    {
        const string input = "This, is tokenizer test! 愛";
        var expectedTokens = new List<string> { "THIS", ",", "IS", "TOKENIZER", "TEST", "!", "愛" };
        var tokenizer = new Tokenizer(new List<char>(input));

        var tokens = tokenizer.ToList();

        Assert.Equal(expectedTokens, tokens);
    }

    [Fact]
    public void Tokenizer_InputContainsDigits_ReturnTokensWithDigitsAsSeparateTokens()
    {
        const string input = "This43 is 34 tokenizer test65";
        var expectedTokens = new List<string> { "THIS", "43", "IS", "34", "TOKENIZER", "TEST", "65" };
        var tokenizer = new Tokenizer(new List<char>(input));

        var tokens = tokenizer.ToList();

        Assert.Equal(expectedTokens, tokens);
    }

    [Fact]
    public void Tokenizer_InputIsEmpty_ReturnZeroElements()
    {
        const string input = "";
        var tokenizer = new Tokenizer(new List<char>(input));

        var tokens = tokenizer.ToList();

        Assert.False(tokens.Any());
    }
}