using System.Globalization;
using System.Text;
using Import.Core;
using Import.Core.Readers;
using Import.Core.Readers.Csv;
using Import.Core.Readers.FixedWidth;
using Import.Core.Transactions;
using Import.Core.Values;
using Import.Core.Writers;

namespace Import.Tests;

/// <summary>Each kind of change is an addition: no existing class of Core is modified.</summary>
public class ExtensibilityTests
{
    [Fact]
    public void New_value_type_only_needs_a_new_value_parser()
    {
        var layout = new FixedWidthLayout(
        [
            new FixedWidthFieldMapping("Id", 1, 4, ValueParsers.Text),
            new FixedWidthFieldMapping("Active", 5, 5, new YesNoParser()),
        ]);

        var json = new ImportPipeline(new FixedWidthRecordReader(layout), new JsonRecordWriter()).Run("F001Y\nF002N");

        Assert.Equal("""[{"Id":"F001","Active":true},{"Id":"F002","Active":false}]""", json);
    }

    [Fact]
    public void New_field_only_needs_a_new_mapping()
    {
        var layout = new CsvLayout(
            TransactionImport.CsvLayout.Fields.Append(new CsvFieldMapping("Currency", 5, ValueParsers.Text)));

        var json = new ImportPipeline(new CsvRecordReader(layout), new JsonRecordWriter())
            .Run("F001,2026-10-10,5050.89,0.145,EUR");

        Assert.EndsWith(""","Currency":"EUR"}]""", json);
    }

    [Fact]
    public void New_input_format_only_needs_a_new_reader_and_a_registration()
    {
        var readers = TransactionImport.CreateReaderRegistry()
            .Register("keyvalue", () => new KeyValueRecordReader(), ".kv");

        var json = new ImportPipeline(readers.CreateForFile("transactions.kv"), new JsonRecordWriter())
            .Run("OriginalSourceId=F001;Rate=0.145");

        Assert.Equal("""[{"OriginalSourceId":"F001","Rate":"0.145"}]""", json);
    }

    [Fact]
    public void New_output_format_only_needs_a_new_writer_and_a_registration()
    {
        var writers = TransactionImport.CreateWriterRegistry()
            .Register("lines", () => new LineWriter(), ".log");

        var pipeline = new ImportPipeline(
            TransactionImport.CreateReaderRegistry().Create(TransactionImport.CsvFormat),
            writers.Create("lines"));

        Assert.Equal(
            "OriginalSourceId=F001|TransactionDate=2026-10-10|Value=5050.89|Rate=0.145\n",
            pipeline.Run("F001,2026-10-10,5050.89,0.145"));
    }

    // ---------- Extensions written OUTSIDE Core ----------

    private sealed class YesNoParser : IValueParser
    {
        public object? Parse(string raw) => raw switch
        {
            "Y" => true,
            "N" => false,
            _ => throw new FormatException("Expected Y or N.")
        };
    }

    /// <summary>A format that is not column based: "key=value;key=value".</summary>
    private sealed class KeyValueRecordReader : IRecordReader
    {
        public IEnumerable<ImportRecord> Read(TextReader input)
        {
            while (input.ReadLine() is { } line)
            {
                var record = new ImportRecord();
                foreach (var pair in line.Split(';'))
                {
                    var parts = pair.Split('=', 2);
                    record.Add(parts[0], parts[1]);
                }
                yield return record;
            }
        }
    }

    private sealed class LineWriter : IRecordWriter
    {
        public void Write(IEnumerable<ImportRecord> records, Stream output)
        {
            using var writer = new StreamWriter(output, new UTF8Encoding(false), leaveOpen: true) { NewLine = "\n" };
            foreach (var record in records)
                writer.WriteLine(string.Join('|', record.Select(f => $"{f.Key}={Format(f.Value)}")));
        }

        private static string? Format(object? value) => value switch
        {
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value?.ToString()
        };
    }
}