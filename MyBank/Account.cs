namespace MyBank;

public class Account
{
    private readonly ITransactionRepository _repository;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly INotificationService _notificationService;

    public string AccountId { get; }
    public decimal Balance { get; private set; }
    public Currency Currency { get; }

    private const decimal LargeTransactionThreshold = 10000;

    public Account(
        ITransactionRepository repository,
        ICurrencyConverter currencyConverter,
        INotificationService notificationService,
        Currency currency = Currency.RON,
        decimal initialBalance = 0)
    {
        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative", nameof(initialBalance));

        AccountId = Guid.NewGuid().ToString();
        Currency = currency;
        Balance = initialBalance;
        
        _repository = repository;
        _currencyConverter = currencyConverter;
        _notificationService = notificationService;

        if (initialBalance > 0)
        {
            var transaction = new Transaction(
                initialBalance,
                currency,
                $"Initial deposit to account {AccountId}");
            _repository.SaveTransaction(transaction);
        }
    }

    public decimal GetBalance()
    {
        return Balance;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive", nameof(amount));

        Balance += amount;

        var transaction = new Transaction(
            amount,
            Currency,
            $"Deposit to account {AccountId}");

        _repository.SaveTransaction(transaction);

        if (amount >= LargeTransactionThreshold)
        {
            _notificationService.SendNotification(
                AccountId,
                $"Large deposit of {amount} {Currency} received");
        }
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Withdrawal amount must be positive", nameof(amount));

        if (amount > Balance)
            throw new InvalidOperationException("Insufficient funds");

        Balance -= amount;

        var transaction = new Transaction(
            amount,
            Currency,
            $"Withdrawal from account {AccountId}");

        _repository.SaveTransaction(transaction);

        if (amount >= LargeTransactionThreshold)
        {
            _notificationService.SendNotification(
                AccountId,
                $"Large withdrawal of {amount} {Currency} processed");
        }
    }

    public void TransferTo(Account targetAccount, decimal amount)
    {
        if (targetAccount == null)
            throw new ArgumentNullException(nameof(targetAccount));

        if (amount <= 0)
            throw new ArgumentException("Transfer amount must be positive", nameof(amount));

        if (amount > Balance)
            throw new InvalidOperationException("Insufficient funds");

        Balance -= amount;

        decimal convertedAmount = amount;
        if (Currency != targetAccount.Currency)
        {
            convertedAmount = _currencyConverter.Convert(amount, Currency, targetAccount.Currency);
        }

        var transferOutTransaction = new Transaction(
            amount,
            Currency,
            $"Transfer out from account {AccountId} to account {targetAccount.AccountId}");

        _repository.SaveTransaction(transferOutTransaction);

        if (amount >= LargeTransactionThreshold)
        {
            _notificationService.SendNotification(
                AccountId,
                $"Large transfer of {amount} {Currency} sent to account {targetAccount.AccountId}");
        }

        targetAccount.ReceiveTransfer(convertedAmount, AccountId);
    }

    private void ReceiveTransfer(decimal amount, string fromAccountId)
    {
        Balance += amount;

        var transferInTransaction = new Transaction(
            amount,
            Currency,
            $"Transfer in to account {AccountId} from account {fromAccountId}");

        _repository.SaveTransaction(transferInTransaction);

        if (amount >= LargeTransactionThreshold)
        {
            _notificationService.SendNotification(
                AccountId,
                $"Large transfer of {amount} {Currency} received from account {fromAccountId}");
        }
    }

    public List<Transaction> GetTransactionHistory()
    {
        return _repository.GetTransactionHistory();
    }
}