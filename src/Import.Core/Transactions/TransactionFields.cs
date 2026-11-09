namespace Import.Core.Transactions;

/// <summary>Business field names, shared by every layout so all formats produce the same output.</summary>
public static class TransactionFields
{
    public const string OriginalSourceId = nameof(OriginalSourceId);
    public const string TransactionDate = nameof(TransactionDate);
    public const string Value = nameof(Value);
    public const string Rate = nameof(Rate);
}