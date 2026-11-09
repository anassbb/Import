using Import.Core;
using Import.Core.Readers.Csv;
using Import.Core.Transactions;
using Import.Core.Writers;
using System.Text;

namespace Import.Tests;

/// <summary>Acceptance tests: the examples of the specification.</summary>
public class TransactionImportTests
{
    private const string ExpectedJson =
        """[{"OriginalSourceId":"F001","TransactionDate":"2026-10-10","Value":5050.89,"Rate":0.145},{"OriginalSourceId":"F003","TransactionDate":"2026-10-20","Value":12200.34,"Rate":1.25}]""";

    private const string CsvInput =
        "F001,2026-10-10,5050.89,0.145\n" +
        "F003,2026-10-20,12200.34,1.250\n";

    private const string FixedWidthInput =
        "F001202610100050508900145\n" +
        "F003202610200122003401250\n";

    [Fact]
    public void Csv_input_produces_expected_json()
    {
        Assert.Equal(ExpectedJson, TransactionImport.CreateCsvPipeline().Run(CsvInput));
    }

    [Fact]
    public void FixedWidth_input_produces_expected_json()
    {
        Assert.Equal(ExpectedJson, TransactionImport.CreateFixedWidthPipeline().Run(FixedWidthInput));
    }

    [Fact]
    public void Both_formats_produce_the_same_json()
    {
        Assert.Equal(
            TransactionImport.CreateCsvPipeline().Run(CsvInput),
            TransactionImport.CreateFixedWidthPipeline().Run(FixedWidthInput));
    }

    [Fact]
    public void Windows_line_endings_are_supported()
    {
        var json = TransactionImport.CreateCsvPipeline().Run(CsvInput.Replace("\n", "\r\n"));

        Assert.Equal(ExpectedJson, json);
    }

    [Fact]
    public void Blank_lines_are_ignored()
    {
        var json = TransactionImport.CreateFixedWidthPipeline().Run("\n" + FixedWidthInput + "\n   \n");

        Assert.Equal(ExpectedJson, json);
    }

    [Fact]
    public void Empty_input_produces_empty_json_array()
    {
        Assert.Equal("[]", TransactionImport.CreateCsvPipeline().Run(""));
        Assert.Equal("[]", TransactionImport.CreateFixedWidthPipeline().Run(""));
    }

    [Fact]
    public void Invalid_line_reports_line_number_and_field()
    {
        var input = "F001,2026-10-10,5050.89,0.145\n\nF003,not-a-date,12200.34,1.250\n";

        var ex = Assert.Throws<ImportException>(() => TransactionImport.CreateCsvPipeline().Run(input));

        Assert.Equal(3, ex.LineNumber);
        Assert.Equal(TransactionFields.TransactionDate, ex.FieldName);
    }

    [Fact]
    public void Pipeline_rejects_null_arguments()
    {
        var reader = new CsvRecordReader(TransactionImport.CsvLayout);
        var writer = new JsonRecordWriter();

        Assert.Throws<ArgumentNullException>(() => new ImportPipeline(null!, writer));
        Assert.Throws<ArgumentNullException>(() => new ImportPipeline(reader, null!));
    }

    [Theory]
    [InlineData("transactions.csv")]
    [InlineData("transactions.txt")]
    public void Sample_files_produce_expected_json(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "samples", fileName);
        var reader = TransactionImport.CreateReaderRegistry().CreateForFile(path);
        var pipeline = new ImportPipeline(reader, new JsonRecordWriter());

        using var input = File.OpenText(path);
        using var output = new MemoryStream();
        pipeline.Run(input, output);

        Assert.Equal(ExpectedJson, Encoding.UTF8.GetString(output.ToArray()));
    }
}