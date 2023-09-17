using System.Reflection;

namespace Indexer;

public enum SupportedExtension
{
    [StringValue(".txt")] Txt,
    [StringValue(".md")] Md,
}

[AttributeUsage((AttributeTargets.Field))]
public sealed class StringValueAttribute : Attribute
{
    public string Value { get; }

    public StringValueAttribute(string value)
    {
        Value = value;
    }
}

public static class EnumExtensions
{
    public static string StringValue<T>(this T value) where T : Enum
    {
        var fieldName = value.ToString();
        var field = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        return field?.GetCustomAttribute<StringValueAttribute>()?.Value ?? fieldName;
    }
}