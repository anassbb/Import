namespace Import.Core.Values;

/// <summary>
/// Converts the raw text of a field into a typed value.
/// Throws FormatException or OverflowException on invalid input.
/// </summary>
public interface IValueParser
{
    object? Parse(string raw);
}