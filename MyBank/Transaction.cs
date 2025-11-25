namespace MyBank;

public class Transaction
{
    public string Id { get; }
    public DateTime Date { get; }
    public decimal Amount { get; }
    public Currency Currency { get; }
    public string Description { get; }

    public Transaction(
        decimal amount,
        Currency currency,
        string description)
    {
        Id = Guid.NewGuid().ToString();
        Date = DateTime.UtcNow;
        Amount = amount;
        Currency = currency;
        Description = description;
    }
}