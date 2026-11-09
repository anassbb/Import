using Import.Core.Values;

namespace Import.Core.Readers;

/// <summary>Links an output field name to the parser of its raw value. Each format adds where to find it.</summary>
public interface IFieldMapping
{
    string Name { get; }

    IValueParser Parser { get; }
}