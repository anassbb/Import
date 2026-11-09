namespace Import.Core.Values;

public sealed class StringValueParser(bool trim = true) : IValueParser
{
    public object? Parse(string raw) => trim ? raw.Trim() : raw;
}