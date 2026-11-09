namespace Import.Core.Values;

/// <summary>Shortcuts to keep layout declarations readable.</summary>
public static class ValueParsers
{
    public static IValueParser Text { get; } = new StringValueParser();
    public static IValueParser DecimalNumber { get; } = new DecimalValueParser();
    public static IValueParser Date(string format) => new DateValueParser(format);
    public static IValueParser ImpliedDecimal(int impliedDecimals) => new ImpliedDecimalValueParser(impliedDecimals);
}