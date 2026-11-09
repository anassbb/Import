using System.Globalization;

namespace Import.Core.Values;

public sealed class DecimalValueParser : IValueParser
{
    // Only a sign and a dot: no thousands separator, no exponent.
    private const NumberStyles Styles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

    public object? Parse(string raw)
        => decimal.Parse(raw.Trim(), Styles, CultureInfo.InvariantCulture);
}