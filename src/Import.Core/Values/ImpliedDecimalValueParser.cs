using System.Globalization;

namespace Import.Core.Values;

/// <summary>
/// Digits with implied decimals: "00505089" with 2 implied decimals -> 5050.89.
/// </summary>
public sealed class ImpliedDecimalValueParser : IValueParser
{
    // A long holds 18 full digits: more implied decimals would make no sense.
    private const int MaxDecimals = 18;

    private readonly decimal _divisor;

    public ImpliedDecimalValueParser(int impliedDecimals)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(impliedDecimals);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(impliedDecimals, MaxDecimals);

        ImpliedDecimals = impliedDecimals;

        // 10^n computed in decimal (Math.Pow returns a double: precision loss).
        _divisor = 1m;
        for (var i = 0; i < impliedDecimals; i++)
            _divisor *= 10m;
    }

    public int ImpliedDecimals { get; }

    public object? Parse(string raw)
        => long.Parse(raw.Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture) / _divisor;
}