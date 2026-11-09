using Import.Core;
using Import.Core.Readers.Csv;
using Import.Core.Transactions;
using Import.Core.Values;

namespace Import.Tests;

public class CsvRecordReaderTests
{
    
    private static readonly CsvLayout Layout = TransactionImport.CsvLayout;

    private readonly CsvRecordReader _reader = new(Layout);

    [Fact]
    public void Parses_all_fields_with_their_types()
    {
        var record = _reader.ReadSingle("F001,2026-10-10,5050.89,0.145");

        Assert.Equal("F001", record["OriginalSourceId"]);
        Assert.Equal(new DateOnly(2026, 10, 10), record["TransactionDate"]);
        Assert.Equal(5050.89m, record["Value"]);
        Assert.Equal(0.145m, record["Rate"]);
    }

    [Fact]
    public void Keeps_fields_in_layout_order()
    {
        var record = _reader.ReadSingle("F001,2026-10-10,5050.89,0.145");

        Assert.Equal(["OriginalSourceId", "TransactionDate", "Value", "Rate"], record.Keys);
    }

    [Fact]
    public void Trims_spaces_around_values()
    {
        var record = _reader.ReadSingle(" F001 , 2026-10-10 , 5050.89 , 0.145 ");

        Assert.Equal("F001", record["OriginalSourceId"]);
        Assert.Equal(5050.89m, record["Value"]);
    }

    [Theory]
    [InlineData("F001,2026-10-10,5050.89", 3)]
    [InlineData("F001,2026-10-10,50,50.89,0.145", 5)] // virgule décimale : décalerait les valeurs
    public void Rejects_a_wrong_number_of_columns(string line, int found)
    {
        var ex = Assert.Throws<ImportException>(() => _reader.ReadSingle(line));

        Assert.Null(ex.FieldName);
        Assert.Contains($"Expected 4 columns but found {found}", ex.Message);
    }

    [Theory]
    [InlineData("F001,20261010,5050.89,0.145", "TransactionDate")]   // date au format largeur fixe
    [InlineData("F001,2026-13-10,5050.89,0.145", "TransactionDate")] // mois 13
    [InlineData("F001,2026-10-10,abc,0.145", "Value")]
    [InlineData("F001,2026-10-10,5050.89,", "Rate")]                 // valeur vide
    public void Reports_the_invalid_field(string line, string field)
    {
        var ex = Assert.Throws<ImportException>(() => _reader.ReadSingle(line));

        Assert.Equal(field, ex.FieldName);
    }

    [Fact]
    public void Skips_header_when_layout_has_one()
    {
        var layout = new CsvLayout(Layout.Fields, hasHeader: true);

        var records = new CsvRecordReader(layout).ReadAll(
            "OriginalSourceId,TransactionDate,Value,Rate\nF001,2026-10-10,5050.89,0.145");

        Assert.Equal("F001", Assert.Single(records)["OriginalSourceId"]);
    }

    [Fact]
    public void Supports_another_separator()
    {
        var layout = new CsvLayout(Layout.Fields, separator: ';');

        var record = new CsvRecordReader(layout).ReadSingle("F001;2026-10-10;5050.89;0.145");

        Assert.Equal(5050.89m, record["Value"]);
    }

    [Fact]
    public void Can_ignore_unmapped_columns()
    {
        var layout = new CsvLayout([new CsvFieldMapping("Id", 2, ValueParsers.Text)], columnCount: 3);

        var record = new CsvRecordReader(layout).ReadSingle("ignored,F001,ignored");

        Assert.Equal("F001", Assert.Single(record).Value);
    }

    // ---------- Validation de la configuration ----------

    [Fact]
    public void Layout_rejects_empty_fields()
    {
        Assert.Throws<ArgumentException>(() => new CsvLayout([]));
    }

    [Fact]
    public void Layout_rejects_duplicate_names()
    {
        Assert.Throws<ArgumentException>(() => new CsvLayout(
        [
            new CsvFieldMapping("A", 1, ValueParsers.Text),
            new CsvFieldMapping("A", 2, ValueParsers.Text),
        ]));
    }

    [Fact]
    public void Layout_rejects_duplicate_columns()
    {
        Assert.Throws<ArgumentException>(() => new CsvLayout(
        [
            new CsvFieldMapping("A", 1, ValueParsers.Text),
            new CsvFieldMapping("B", 1, ValueParsers.Text),
        ]));
    }

    [Fact]
    public void Layout_rejects_column_count_lower_than_mapped_columns()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CsvLayout([new CsvFieldMapping("A", 3, ValueParsers.Text)], columnCount: 2));
    }

    [Fact]
    public void Mapping_rejects_column_zero()
    {
        // Les colonnes sont numérotées à partir de 1, comme dans la spec.
        Assert.Throws<ArgumentOutOfRangeException>(() => new CsvFieldMapping("A", 0, ValueParsers.Text));
    }
}