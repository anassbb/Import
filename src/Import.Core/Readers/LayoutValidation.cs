namespace Import.Core.Readers;

internal static class LayoutValidation
{
    public static IReadOnlyList<T> EnsureValid<T>(IEnumerable<T> fields, string paramName)
        where T : IFieldMapping
    {
        ArgumentNullException.ThrowIfNull(fields, paramName);
        var list = fields.ToList();

        if (list.Count == 0)
            throw new ArgumentException("A layout must contain at least one field.", paramName);

        var duplicate = list.GroupBy(f => f.Name, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new ArgumentException($"Duplicate field name '{duplicate.Key}'.", paramName);

        return list.AsReadOnly();
    }
}