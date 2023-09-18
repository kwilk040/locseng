using System.Reflection;

namespace Indexer;

internal enum SupportedExtension
{
    [StringValue(".txt")] Txt,
    [StringValue(".md")] Md,
    [StringValue(".html")] Html,
}

[AttributeUsage((AttributeTargets.Field))]
internal sealed class StringValueAttribute : Attribute
{
    public string Value { get; }

    public StringValueAttribute(string value)
    {
        Value = value;
    }
}

internal static class EnumExtensions
{
    public static string StringValue<T>(this T value) where T : Enum
    {
        var fieldName = value.ToString();
        var field = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        return field?.GetCustomAttribute<StringValueAttribute>()?.Value ?? fieldName;
    }
}