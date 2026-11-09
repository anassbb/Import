using System.Text;
using System.Text.Json;
using Import.Core;
using Import.Core.Writers;

namespace Import.Tests;

public class JsonRecordWriterTests
{
    private static string Write(IRecordWriter writer, params ImportRecord[] records)
    {
        using var output = new MemoryStream();
        writer.Write(records, output);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    private static ImportRecord Record(params (string Name, object? Value)[] fields)
    {
        var record = new ImportRecord();
        foreach (var (name, value) in fields)
            record.Add(name, value);
        return record;
    }

    [Fact]
    public void Writes_compact_json_in_field_order()
    {
        var json = Write(new JsonRecordWriter(), Record(("B", "x"), ("A", 1)));

        Assert.Equal("""[{"B":"x","A":1}]""", json);
    }

    [Fact]
    public void Removes_trailing_zeros_from_decimals()
    {
        var json = Write(new JsonRecordWriter(), Record(("Rate", 1.250m), ("Zero", 0.00m), ("Int", 10.0m)));

        Assert.Equal("""[{"Rate":1.25,"Zero":0,"Int":10}]""", json);
    }

    [Fact]
    public void Writes_dates_as_iso_and_nulls_as_null()
    {
        var json = Write(new JsonRecordWriter(), Record(("D", new DateOnly(2026, 10, 10)), ("N", null)));

        Assert.Equal("""[{"D":"2026-10-10","N":null}]""", json);
    }

    [Fact]
    public void Escapes_special_characters()
    {
        var json = Write(new JsonRecordWriter(), Record(("S", "a\"b")));

        // On relit le JSON plutôt que de comparer le texte : l'échappement exact importe peu, la validité oui.
        using var document = JsonDocument.Parse(json);
        Assert.Equal("a\"b", document.RootElement[0].GetProperty("S").GetString());
    }

    [Fact]
    public void Can_write_indented_json()
    {
        var json = Write(new JsonRecordWriter(indented: true), Record(("A", 1)));

        Assert.Contains("\n", json);
    }

    [Fact]
    public void Writes_empty_array_when_no_record()
    {
        Assert.Equal("[]", Write(new JsonRecordWriter()));
    }

    [Fact]
    public void Writes_nothing_when_the_source_fails_early()
    {
        using var output = new MemoryStream();

        Assert.Throws<InvalidOperationException>(() => new JsonRecordWriter().Write(Failing(), output));
        Assert.Equal(0, output.Length); // pas de "[{...}," tronqué

        static IEnumerable<ImportRecord> Failing()
        {
            yield return Record(("A", 1));
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public void Streams_large_outputs()
    {
        var records = Enumerable.Range(0, 20_000).Select(i => Record(("I", i)));

        using var output = new MemoryStream();
        new JsonRecordWriter().Write(records, output);

        using var document = JsonDocument.Parse(output.ToArray());
        Assert.Equal(20_000, document.RootElement.GetArrayLength());
    }

    [Fact]
    public void Leaves_the_output_stream_open()
    {
        using var output = new MemoryStream();

        new JsonRecordWriter().Write([Record(("A", 1))], output);

        Assert.True(output.CanWrite); // c'est l'appelant qui possède le flux, pas l'écrivain
    }
}