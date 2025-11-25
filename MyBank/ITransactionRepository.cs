namespace MyBank;

public interface ITransactionRepository
{
    void SaveTransaction(Transaction transaction);
    List<Transaction> GetTransactionHistory();
}