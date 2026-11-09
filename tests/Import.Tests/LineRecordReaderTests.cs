using Import.Core;
using Import.Core.Readers;
using Import.Core.Values;

namespace Import.Tests;

public class LineRecordReaderTests
{
    private static readonly TestMapping[] Mappings =
    [
        new("Id", 0, ValueParsers.Text),
        new("Value", 1, ValueParsers.DecimalNumber),
    ];

    private static List<ImportRecord> ReadAll(string content, int headerLines = 0)
        => new PipeReader(Mappings, headerLines).Read(new StringReader(content)).ToList();

    [Fact]
    public void Reads_one_record_per_line()
    {
        var records = ReadAll("F001|5050.89\nF003|12200.34");

        Assert.Equal(2, records.Count);
        Assert.Equal("F001", records[0]["Id"]);
        Assert.Equal(5050.89m, records[0]["Value"]);
        Assert.Equal(12200.34m, records[1]["Value"]);
    }

    [Fact]
    public void Skips_blank_lines()
    {
        Assert.Equal(2, ReadAll("\nA|1\n   \nB|2\n").Count);
    }

    [Fact]
    public void Skips_header_lines()
    {
        var record = Assert.Single(ReadAll("Id|Value\nA|1", headerLines: 1));

        Assert.Equal("A", record["Id"]);
    }

    [Fact]
    public void Empty_input_gives_no_record()
    {
        Assert.Empty(ReadAll(""));
    }

    [Fact]
    public void Reports_line_and_field_on_invalid_value()
    {
        // ligne 1 OK, ligne 2 vide, ligne 3 invalide
        var ex = Assert.Throws<ImportException>(() => ReadAll("A|1\n\nB|abc"));

        Assert.Equal(3, ex.LineNumber);
        Assert.Equal("Value", ex.FieldName);
        Assert.IsType<FormatException>(ex.InnerException);
        Assert.StartsWith("Line 3, field 'Value':", ex.Message);
    }

    [Fact]
    public void Reports_line_on_invalid_structure()
    {
        var ex = Assert.Throws<ImportException>(() => ReadAll("A|1|extra"));

        Assert.Equal(1, ex.LineNumber);
        Assert.Null(ex.FieldName);
        Assert.StartsWith("Line 1:", ex.Message);
    }

    [Fact]
    public void Reading_is_lazy()
    {
        var reader = new PipeReader(Mappings, 0);
        using var records = reader.Read(new StringReader("A|1\nBROKEN")).GetEnumerator();

        Assert.True(records.MoveNext());                          // la 1re ligne est lue...
        Assert.Throws<ImportException>(() => records.MoveNext()); // ...l'erreur n'arrive qu'à la 2e
    }

    [Fact]
    public void Rejects_null_input_immediately()
    {
        var reader = new PipeReader(Mappings, 0);

        Assert.Throws<ArgumentNullException>(() => reader.Read(null!)); // sans énumérer
    }

    // ---------- Mini-implémentation pour tester la classe abstraite ----------

    private sealed record TestMapping(string Name, int Index, IValueParser Parser) : IFieldMapping;

    private sealed class PipeReader(IReadOnlyList<TestMapping> mappings, int headerLines)
        : LineRecordReader<TestMapping>(mappings)
    {
        protected override int HeaderLineCount => headerLines;

        protected override Func<TestMapping, string> Split(string line)
        {
            var parts = line.Split('|');
            if (parts.Length != 2)
                throw new FormatException($"Expected 2 parts but found {parts.Length}.");

            return mapping => parts[mapping.Index];
        }
    }
}