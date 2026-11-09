using Import.Core;
using Import.Core.Readers;

namespace Import.Tests;

internal static class TestHelpers
{
    public static List<ImportRecord> ReadAll(this IRecordReader reader, string content)
        => reader.Read(new StringReader(content)).ToList();

    public static ImportRecord ReadSingle(this IRecordReader reader, string line)
        => Assert.Single(reader.ReadAll(line));
}