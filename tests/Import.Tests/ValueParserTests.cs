using System.Globalization;
using Import.Core.Values;

namespace Import.Tests;

public class ValueParserTests
{
    // ---------- String ----------
    [Fact]
    public void String_trims_by_default()
    {
        Assert.Equal("F1", new StringValueParser().Parse(" F1 "));
    }

    [Fact]
    public void String_can_keep_spaces()
    {
        Assert.Equal(" F1 ", new StringValueParser(trim: false).Parse(" F1 "));
    }

    // ---------- Date ----------
    [Theory]
    [InlineData("yyyy-MM-dd", "2026-10-10")]
    [InlineData("yyyyMMdd", "20261010")]
    [InlineData("dd/MM/yyyy", "10/10/2026")]
    public void Date_uses_the_configured_format(string format, string raw)
    {
        Assert.Equal(new DateOnly(2026, 10, 10), new DateValueParser(format).Parse(raw));
    }

    [Theory]
    [InlineData("20261010")]   // bon jour, mauvais format
    [InlineData("2026-13-10")] // mois 13
    [InlineData("")]
    public void Date_rejects_invalid_values(string raw)
    {
        Assert.Throws<FormatException>(() => new DateValueParser("yyyy-MM-dd").Parse(raw));
    }

    [Fact]
    public void Date_requires_a_format()
    {
        Assert.ThrowsAny<ArgumentException>(() => new DateValueParser(""));
    }

    // ---------- Decimal ----------
    [Theory]
    [InlineData("5050.89", "5050.89")]
    [InlineData("-0.5", "-0.5")]
    [InlineData(" 0.145 ", "0.145")]
    public void Decimal_parses_with_a_dot(string raw, string expected)
    {
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), new DecimalValueParser().Parse(raw));
    }

    [Theory]
    [InlineData("5050,89")]  // virgule décimale
    [InlineData("5,050.89")] // séparateur de milliers
    [InlineData("1e3")]
    [InlineData("abc")]
    [InlineData("")]
    public void Decimal_rejects_unexpected_notations(string raw)
    {
        Assert.Throws<FormatException>(() => new DecimalValueParser().Parse(raw));
    }

    [Fact]
    public void Decimal_ignores_the_current_culture()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR"); // en français, le séparateur est ","
            Assert.Equal(5050.89m, new DecimalValueParser().Parse("5050.89"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    // ---------- Implied decimal ----------
    [Theory]
    [InlineData("00505089", 2, "5050.89")]
    [InlineData("01220034", 2, "12200.34")]
    [InlineData("00000000", 2, "0")]
    [InlineData("00145", 3, "0.145")]
    [InlineData("01250", 3, "1.25")]
    [InlineData("-0100", 2, "-1")]
    [InlineData("42", 0, "42")]
    public void ImpliedDecimal_applies_the_scale(string raw, int decimals, string expected)
    {
        var value = new ImpliedDecimalValueParser(decimals).Parse(raw);

        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), value);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(19)]
    public void ImpliedDecimal_rejects_invalid_scale(int decimals)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ImpliedDecimalValueParser(decimals));
    }

    [Theory]
    [InlineData("005050.89")] // un point n'a rien à faire ici
    [InlineData("00 45")]
    [InlineData("ABCD")]
    [InlineData("")]
    public void ImpliedDecimal_rejects_non_digits(string raw)
    {
        Assert.Throws<FormatException>(() => new ImpliedDecimalValueParser(2).Parse(raw));
    }

    [Fact]
    public void ImpliedDecimal_rejects_too_many_digits()
    {
        Assert.Throws<OverflowException>(() => new ImpliedDecimalValueParser(2).Parse("99999999999999999999"));
    }
}