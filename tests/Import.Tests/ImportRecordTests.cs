using Import.Core;

namespace Import.Tests;

public class ImportRecordTests
{
    [Fact]
    public void Keeps_fields_in_insertion_order()
    {
        var record = new ImportRecord();
        record.Add("B", "x");
        record.Add("A", 1);

        Assert.Equal(["B", "A"], record.Keys);
    }

    [Fact]
    public void Gives_access_to_values_by_name()
    {
        var record = new ImportRecord();
        record.Add("Value", 5050.89m);

        Assert.Equal(5050.89m, record["Value"]);
        Assert.Equal(5050.89m, record.Get<decimal>("Value"));
        Assert.True(record.ContainsKey("Value"));
        Assert.Single(record);
    }

    [Fact]
    public void Rejects_duplicate_field()
    {
        var record = new ImportRecord();
        record.Add("A", 1);

        Assert.Throws<ArgumentException>(() => record.Add("A", 2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Rejects_empty_field_name(string? name)
    {
        var record = new ImportRecord();

        Assert.ThrowsAny<ArgumentException>(() => record.Add(name!, 1));
    }
}