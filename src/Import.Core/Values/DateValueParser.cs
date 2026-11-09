using System.Globalization;

namespace Import.Core.Values;

public sealed class DateValueParser : IValueParser
{
    private readonly string _format;

    public DateValueParser(string format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        _format = format;
    }

    public object? Parse(string raw)
        => DateOnly.ParseExact(raw.Trim(), _format, CultureInfo.InvariantCulture);
}