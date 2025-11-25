namespace MyBank;

public interface ICurrencyConverter
{
    decimal Convert(decimal amount, Currency from, Currency to);
}