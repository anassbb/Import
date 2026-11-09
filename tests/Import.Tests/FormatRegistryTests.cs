using Import.Core.Readers;
using Import.Core.Readers.Csv;
using Import.Core.Readers.FixedWidth;
using Import.Core.Registry;
using Import.Core.Transactions;
using Import.Core.Writers;

namespace Import.Tests;

public class FormatRegistryTests
{
    private readonly FormatRegistry<IRecordReader> _readers = TransactionImport.CreateReaderRegistry();

    [Fact]
    public void Creates_registered_format_ignoring_case()
    {
        Assert.IsType<CsvRecordReader>(_readers.Create("csv"));
        Assert.IsType<CsvRecordReader>(_readers.Create("CSV"));
        Assert.IsType<FixedWidthRecordReader>(_readers.Create("fixed"));
    }

    [Theory]
    [InlineData("data.csv", "csv")]
    [InlineData(@"C:\in\DATA.CSV", "csv")]
    [InlineData("data.txt", "fixed")]
    [InlineData("data.dat", "fixed")]
    public void Detects_format_from_file_extension(string path, string expected)
    {
        Assert.Equal(expected, _readers.DetectFormat(path));
    }

    [Fact]
    public void Lists_registered_formats()
    {
        Assert.Equal(["csv", "fixed"], _readers.Formats.Order());
    }

    [Fact]
    public void Unknown_format_lists_the_available_ones()
    {
        var ex = Assert.Throws<NotSupportedException>(() => _readers.Create("xml"));

        Assert.Contains("csv, fixed", ex.Message);
    }

    [Fact]
    public void Unknown_extension_is_rejected()
    {
        Assert.Throws<NotSupportedException>(() => _readers.DetectFormat("data.xlsx"));
    }

    [Fact]
    public void Extension_can_be_registered_without_dot()
    {
        var registry = new FormatRegistry<IRecordWriter>().Register("json", () => new JsonRecordWriter(), "json");

        Assert.Equal("json", registry.DetectFormat("out.json"));
    }

    [Fact]
    public void Rejects_a_format_registered_twice()
    {
        Assert.Throws<ArgumentException>(
            () => _readers.Register("CSV", () => new CsvRecordReader(TransactionImport.CsvLayout)));
    }

    [Fact]
    public void Rejects_an_extension_already_used()
    {
        Assert.Throws<ArgumentException>(
            () => _readers.Register("other", () => new CsvRecordReader(TransactionImport.CsvLayout), ".CSV"));
    }
}