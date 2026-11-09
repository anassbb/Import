using Import.Core;
using Import.Core.Readers.FixedWidth;
using Import.Core.Transactions;
using Import.Core.Values;

namespace Import.Tests;

public class FixedWidthRecordReaderTests
{
    private static readonly FixedWidthLayout Layout = TransactionImport.FixedWidthLayout;

    private readonly FixedWidthRecordReader _reader = new(Layout);

    [Fact]
    public void Parses_all_fields_with_their_types()
    {
        var record = _reader.ReadSingle("F001202610100050508900145");

        Assert.Equal("F001", record["OriginalSourceId"]);
        Assert.Equal(new DateOnly(2026, 10, 10), record["TransactionDate"]);
        Assert.Equal(5050.89m, record["Value"]);
        Assert.Equal(0.145m, record["Rate"]);
    }

    [Fact]
    public void Layout_matches_the_specification()
    {
        Assert.Equal([4, 8, 8, 5], Layout.Fields.Select(f => f.Length));
        Assert.Equal(25, Layout.MinimumLineLength);
    }

    [Fact]
    public void Trims_padded_strings()
    {
        Assert.Equal("F1", _reader.ReadSingle("F1  202610100050508900145")["OriginalSourceId"]);
    }

    [Fact]
    public void Ignores_characters_after_the_last_field()
    {
        Assert.Equal(0.145m, _reader.ReadSingle("F001202610100050508900145   EXTRA")["Rate"]);
    }

    [Fact]
    public void Rejects_a_line_that_is_too_short()
    {
        var ex = Assert.Throws<ImportException>(() => _reader.ReadSingle("F00120261010005050890014")); // 24 car.

        Assert.Null(ex.FieldName);
        Assert.Contains("at least 25 characters", ex.Message);
    }

    [Theory]
    [InlineData("F001202613100050508900145", "TransactionDate")] // mois 13
    [InlineData("F0012026-10-0050508900145", "TransactionDate")] // date au format CSV
    [InlineData("F00120261010005050AB00145", "Value")]
    [InlineData("F001202610100050508900 45", "Rate")]
    public void Reports_the_invalid_field(string line, string field)
    {
        var ex = Assert.Throws<ImportException>(() => _reader.ReadSingle(line));

        Assert.Equal(field, ex.FieldName);
    }

    // ---------- Validation de la configuration ----------

    [Fact]
    public void Layout_rejects_overlapping_fields()
    {
        var ex = Assert.Throws<ArgumentException>(() => new FixedWidthLayout(
        [
            new FixedWidthFieldMapping("A", 1, 4, ValueParsers.Text),
            new FixedWidthFieldMapping("B", 4, 8, ValueParsers.Text), // le caractère 4 est pris deux fois
        ]));

        Assert.Contains("overlap", ex.Message);
    }

    [Fact]
    public void Layout_rejects_duplicate_names()
    {
        Assert.Throws<ArgumentException>(() => new FixedWidthLayout(
        [
            new FixedWidthFieldMapping("A", 1, 2, ValueParsers.Text),
            new FixedWidthFieldMapping("A", 3, 4, ValueParsers.Text),
        ]));
    }

    [Theory]
    [InlineData(0, 4)] // les positions commencent à 1
    [InlineData(5, 4)] // fin avant début
    public void Mapping_rejects_invalid_range(int start, int end)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new FixedWidthFieldMapping("A", start, end, ValueParsers.Text));
    }
}