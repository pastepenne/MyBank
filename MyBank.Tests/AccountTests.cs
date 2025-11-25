using Moq;

namespace MyBank.Tests;

public class AccountTests
{
    #region Domain Testing
    [Test]
    public void Constructor_ValidInitialBalance_CreatesAccountWithCorrectBalance()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        decimal initialBalance = 1000m;

        // Act
        var account = new Account(
            mockRepository.Object,
            mockConverter.Object,
            mockNotification.Object,
            Currency.RON,
            initialBalance);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(1000m));
        Assert.That(account.Currency, Is.EqualTo(Currency.RON));
    }

    [TestCase(Currency.RON, 1000)]
    [TestCase(Currency.EUR, 500)]
    [TestCase(Currency.USD, 2500.50)]
    [TestCase(Currency.GBP, 0.01)]
    [TestCase(Currency.EUR, 99999.99)]
    public void Constructor_VariousCurrenciesAndBalances_CreatesAccountCorrectly(Currency currency, decimal initialBalance)
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act
        var account = new Account(
            mockRepository.Object,
            mockConverter.Object,
            mockNotification.Object,
            currency,
            initialBalance);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(initialBalance));
        Assert.That(account.Currency, Is.EqualTo(currency));
    }

    [Test]
    public void Constructor_NegativeInitialBalance_ThrowsArgumentException()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Account(
            mockRepository.Object,
            mockConverter.Object,
            mockNotification.Object,
            Currency.EUR,
            -100m));
        
        Assert.That(ex.ParamName, Is.EqualTo("initialBalance"));
    }

    [TestCase(-100)]
    [TestCase(-0.01)]
    [TestCase(-1000)]
    [TestCase(-999999)]
    public void Constructor_VariousNegativeBalances_ThrowsArgumentException(decimal negativeBalance)
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => new Account(
            mockRepository.Object,
            mockConverter.Object,
            mockNotification.Object,
            Currency.EUR,
            negativeBalance));
        
        Assert.That(ex.ParamName, Is.EqualTo("initialBalance"));
    }

    [Test]
    public void Deposit_PositiveAmount_IncreasesBalance()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 500m);

        // Act
        account.Deposit(300m);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(800m));
    }

    [Test]
    public void Deposit_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => account.Deposit(0m));
        Assert.That(ex.ParamName, Is.EqualTo("amount"));
    }

    [Test]
    public void Deposit_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => account.Deposit(-50m));
        Assert.That(ex.ParamName, Is.EqualTo("amount"));
    }

    [Test]
    public void Withdraw_SufficientBalance_DecreasesBalance()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);

        // Act
        account.Withdraw(300m);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(700m));
    }

    [Test]
    public void Withdraw_InsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 100m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => account.Withdraw(500m));
    }

    [Test]
    public void Withdraw_ExactBalance_ReducesBalanceToZero()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 250m);

        // Act
        account.Withdraw(250m);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(0m));
    }

    [Test]
    public void Withdraw_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => account.Withdraw(0m));
        Assert.That(ex.ParamName, Is.EqualTo("amount"));
    }

    [Test]
    public void Withdraw_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => account.Withdraw(-50m));
        Assert.That(ex.ParamName, Is.EqualTo("amount"));
    }

    [Test]
    public void GetBalance_AfterMultipleOperations_ReturnsCorrectBalance()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);

        // Act
        account.Deposit(500m);
        account.Withdraw(200m);
        account.Deposit(100m);

        // Assert
        Assert.That(account.GetBalance(), Is.EqualTo(1400m));
    }
    #endregion

    #region Test Doubles
    [Test]
    public void Deposit_WithStubRepository_SavesTransactionCorrectly()
    {
        // Arrange
        var stubRepository = new StubTransactionRepository();
        var stubConverter = new StubCurrencyConverter();
        var stubNotification = new StubNotificationService();
        var account = new Account(stubRepository, stubConverter, stubNotification, Currency.RON, 500m);

        // Act
        account.Deposit(200m);

        // Assert
        Assert.That(stubRepository.SavedTransactions.Count, Is.EqualTo(2)); // Initial + Deposit
        Assert.That(stubRepository.SavedTransactions[1].Amount, Is.EqualTo(200m));
        Assert.That(stubRepository.SavedTransactions[1].Currency, Is.EqualTo(Currency.RON));
        Assert.That(stubRepository.SavedTransactions[1].Description, Does.Contain("Deposit"));
    }

    [Test]
    public void Withdraw_WithStubRepository_SavesTransactionCorrectly()
    {
        // Arrange
        var stubRepository = new StubTransactionRepository();
        var stubConverter = new StubCurrencyConverter();
        var stubNotification = new StubNotificationService();
        var account = new Account(stubRepository, stubConverter, stubNotification, Currency.EUR, 1000m);

        // Act
        account.Withdraw(300m);

        // Assert
        Assert.That(stubRepository.SavedTransactions.Count, Is.EqualTo(2)); // Initial + Withdraw
        Assert.That(stubRepository.SavedTransactions[1].Amount, Is.EqualTo(300m));
        Assert.That(stubRepository.SavedTransactions[1].Description, Does.Contain("Withdrawal"));
    }

    [Test]
    public void TransferTo_SameCurrency_TransfersCorrectAmount()
    {
        // Arrange
        var stubRepository1 = new StubTransactionRepository();
        var stubRepository2 = new StubTransactionRepository();
        var stubConverter = new StubCurrencyConverter();
        var stubNotification = new StubNotificationService();
        
        var account1 = new Account(stubRepository1, stubConverter, stubNotification, Currency.RON, 1000m);
        var account2 = new Account(stubRepository2, stubConverter, stubNotification, Currency.RON, 500m);

        // Act
        account1.TransferTo(account2, 300m);

        // Assert
        Assert.That(account1.Balance, Is.EqualTo(700m));
        Assert.That(account2.Balance, Is.EqualTo(800m));
        Assert.That(stubRepository1.SavedTransactions.Last().Description, Does.Contain("Transfer out"));
        Assert.That(stubRepository2.SavedTransactions.Last().Description, Does.Contain("Transfer in"));
    }

    [Test]
    public void TransferTo_DifferentCurrency_UsesConverterStub()
    {
        // Arrange
        var stubRepository1 = new StubTransactionRepository();
        var stubRepository2 = new StubTransactionRepository();
        var stubConverter = new StubCurrencyConverter(); // Returns amount * 5 for RON to EUR
        var stubNotification = new StubNotificationService();
        
        var accountRON = new Account(stubRepository1, stubConverter, stubNotification, Currency.RON, 1000m);
        var accountEUR = new Account(stubRepository2, stubConverter, stubNotification, Currency.EUR, 100m);

        // Act
        accountRON.TransferTo(accountEUR, 100m); // 100 RON -> 500 EUR (stub conversion)

        // Assert
        Assert.That(accountRON.Balance, Is.EqualTo(900m));
        Assert.That(accountEUR.Balance, Is.EqualTo(600m)); // 100 + 500
    }

    [Test]
    public void GetTransactionHistory_WithStubRepository_ReturnsAllTransactions()
    {
        // Arrange
        var stubRepository = new StubTransactionRepository();
        var stubConverter = new StubCurrencyConverter();
        var stubNotification = new StubNotificationService();
        var account = new Account(stubRepository, stubConverter, stubNotification, Currency.RON, 1000m);

        // Act
        account.Deposit(200m);
        account.Withdraw(100m);
        var history = account.GetTransactionHistory();

        // Assert
        Assert.That(history.Count(), Is.EqualTo(3)); // Initial, Deposit, Withdraw
    }

    [Test]
    public void Deposit_LargeAmountWithStub_AddsNotificationToStubService()
    {
        // Arrange
        var stubRepository = new StubTransactionRepository();
        var stubConverter = new StubCurrencyConverter();
        var stubNotification = new StubNotificationService();
        var account = new Account(stubRepository, stubConverter, stubNotification, Currency.USD, 5000m);

        // Act
        account.Deposit(15000m); // Above 10000 threshold

        // Assert
        Assert.That(stubNotification.SentNotifications.Count, Is.EqualTo(1));
        Assert.That(stubNotification.SentNotifications[0], Does.Contain(account.AccountId));
        Assert.That(stubNotification.SentNotifications[0], Does.Contain("Large deposit"));
    }
    #endregion

    #region Mocking Framework
    [Test]
    public void Deposit_SmallAmount_DoesNotTriggerNotification()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 500m);

        // Act
        account.Deposit(100m); // Less than 10000 threshold

        // Assert
        mockNotification.Verify(n => n.SendNotification(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Deposit_LargeAmount_TriggersNotification()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 5000m);

        // Act
        account.Deposit(15000m); // Above 10000 threshold

        // Assert
        mockNotification.Verify(
            n => n.SendNotification(account.AccountId, It.Is<string>(s => s.Contains("Large deposit"))),
            Times.Once);
    }

    [TestCase(10000)]
    [TestCase(20000)]
    [TestCase(50000)]
    [TestCase(10000.01)]
    public void Withdraw_LargeAmount_TriggersNotification(decimal largeAmount)
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.EUR, 100000m);

        // Act
        account.Withdraw(largeAmount); // Above or equal 10000 threshold

        // Assert
        mockNotification.Verify(
            n => n.SendNotification(account.AccountId, It.Is<string>(s => s.Contains("Large withdrawal"))),
            Times.Once);
    }

    [Test]
    public void TransferTo_LargeAmount_TriggersBothNotifications()
    {
        // Arrange
        var mockRepository1 = new Mock<ITransactionRepository>();
        var mockRepository2 = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        
        var account1 = new Account(mockRepository1.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 50000m);
        var account2 = new Account(mockRepository2.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);

        // Act
        account1.TransferTo(account2, 15000m); // Above 10000 threshold

        // Assert
        mockNotification.Verify(
            n => n.SendNotification(account1.AccountId, It.Is<string>(s => s.Contains("Large transfer") && s.Contains("sent"))),
            Times.Once);
        mockNotification.Verify(
            n => n.SendNotification(account2.AccountId, It.Is<string>(s => s.Contains("Large transfer") && s.Contains("received"))),
            Times.Once);
    }

    [Test]
    public void Deposit_AnyAmount_SavesTransactionToRepository()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.USD, 1000m);

        // Act
        account.Deposit(500m);

        // Assert
        mockRepository.Verify(
            r => r.SaveTransaction(It.Is<Transaction>(t => 
                t.Amount == 500m && 
                t.Currency == Currency.USD &&
                t.Description.Contains("Deposit"))),
            Times.Once);
    }

    [Test]
    public void TransferTo_DifferentCurrency_CallsConverter()
    {
        // Arrange
        var mockRepository1 = new Mock<ITransactionRepository>();
        var mockRepository2 = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        mockConverter.Setup(c => c.Convert(100m, Currency.RON, Currency.EUR)).Returns(20m);
        var mockNotification = new Mock<INotificationService>();
        
        var accountRON = new Account(mockRepository1.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);
        var accountEUR = new Account(mockRepository2.Object, mockConverter.Object, mockNotification.Object, Currency.EUR, 500m);

        // Act
        accountRON.TransferTo(accountEUR, 100m);

        // Assert
        mockConverter.Verify(c => c.Convert(100m, Currency.RON, Currency.EUR), Times.Once);
        Assert.That(accountEUR.Balance, Is.EqualTo(520m)); // 500 + 20
    }

    [Test]
    public void TransferTo_SameCurrency_DoesNotCallConverter()
    {
        // Arrange
        var mockRepository1 = new Mock<ITransactionRepository>();
        var mockRepository2 = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        
        var account1 = new Account(mockRepository1.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);
        var account2 = new Account(mockRepository2.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 500m);

        // Act
        account1.TransferTo(account2, 100m);

        // Assert
        mockConverter.Verify(c => c.Convert(It.IsAny<decimal>(), It.IsAny<Currency>(), It.IsAny<Currency>()), Times.Never);
    }

    [Test]
    public void Constructor_WithInitialBalance_SavesInitialTransaction()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);

        // Assert
        mockRepository.Verify(
            r => r.SaveTransaction(It.Is<Transaction>(t => 
                t.Amount == 1000m && 
                t.Description.Contains("Initial deposit"))),
            Times.Once);
    }

    [Test]
    public void GetTransactionHistory_WhenCalled_CallsRepository()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var transactions = new List<Transaction>
        {
            new Transaction(100m, Currency.RON, "Test transaction")
        };
        mockRepository.Setup(r => r.GetTransactionHistory()).Returns(transactions);
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON);

        // Act
        var history = account.GetTransactionHistory();

        // Assert
        mockRepository.Verify(r => r.GetTransactionHistory(), Times.Once);
        Assert.That(history.Count(), Is.EqualTo(1));
    }

    [Test]
    public void Constructor_ZeroInitialBalance_DoesNotSaveTransaction()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.USD, 0m);

        // Assert
        mockRepository.Verify(r => r.SaveTransaction(It.IsAny<Transaction>()), Times.Never);
    }

    [Test]
    public void TransferTo_NullTargetAccount_ThrowsArgumentNullException()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => account.TransferTo(null!, 100m));
    }

    [Test]
    public void TransferTo_ZeroAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepository1 = new Mock<ITransactionRepository>();
        var mockRepository2 = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account1 = new Account(mockRepository1.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);
        var account2 = new Account(mockRepository2.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 500m);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => account1.TransferTo(account2, 0m));
        Assert.That(ex.ParamName, Is.EqualTo("amount"));
    }

    [Test]
    public void TransferTo_NegativeAmount_ThrowsArgumentException()
    {
        // Arrange
        var mockRepository1 = new Mock<ITransactionRepository>();
        var mockRepository2 = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account1 = new Account(mockRepository1.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 1000m);
        var account2 = new Account(mockRepository2.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 500m);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => account1.TransferTo(account2, -100m));
        Assert.That(ex.ParamName, Is.EqualTo("amount"));
    }

    [Test]
    public void TransferTo_InsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepository1 = new Mock<ITransactionRepository>();
        var mockRepository2 = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account1 = new Account(mockRepository1.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 100m);
        var account2 = new Account(mockRepository2.Object, mockConverter.Object, mockNotification.Object, Currency.RON, 500m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => account1.TransferTo(account2, 500m));
    }

    [Test]
    public void Deposit_MultipleDeposits_AccumulatesBalance()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.EUR, 100m);

        // Act
        account.Deposit(50m);
        account.Deposit(75m);
        account.Deposit(25m);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(250m));
        mockRepository.Verify(r => r.SaveTransaction(It.IsAny<Transaction>()), Times.Exactly(4)); // Initial + 3 deposits
    }

    [Test]
    public void Withdraw_MultipleWithdrawals_DecreasesBalance()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.GBP, 1000m);

        // Act
        account.Withdraw(100m);
        account.Withdraw(200m);
        account.Withdraw(150m);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(550m));
    }

    [Test]
    public void Constructor_WithDefaultParameters_CreatesAccountWithRONAndZeroBalance()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object);

        // Assert
        Assert.That(account.Balance, Is.EqualTo(0m));
        Assert.That(account.Currency, Is.EqualTo(Currency.RON));
        Assert.That(account.AccountId, Is.Not.Null);
    }

    [Test]
    public void AccountId_AfterCreation_IsNotEmpty()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act
        var account = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object, Currency.USD);

        // Assert
        Assert.That(account.AccountId, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void AccountId_MultipleAccounts_AreUnique()
    {
        // Arrange
        var mockRepository = new Mock<ITransactionRepository>();
        var mockConverter = new Mock<ICurrencyConverter>();
        var mockNotification = new Mock<INotificationService>();

        // Act
        var account1 = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object);
        var account2 = new Account(mockRepository.Object, mockConverter.Object, mockNotification.Object);

        // Assert
        Assert.That(account1.AccountId, Is.Not.EqualTo(account2.AccountId));
    }
    #endregion
}

#region Stub Implementations
public class StubTransactionRepository : ITransactionRepository
{
    public List<Transaction> SavedTransactions { get; } = [];

    public void SaveTransaction(Transaction transaction)
    {
        SavedTransactions.Add(transaction);
    }

    public List<Transaction> GetTransactionHistory()
    {
        return SavedTransactions;
    }
}

public class StubCurrencyConverter : ICurrencyConverter
{
    public decimal Convert(decimal amount, Currency from, Currency to)
    {
        return amount * 5;
    }
}

public class StubNotificationService : INotificationService
{
    public List<string> SentNotifications { get; } = [];

    public void SendNotification(string accountId, string message)
    {
        SentNotifications.Add($"{accountId}: {message}");
    }
}
#endregion
