namespace MyBank;

public interface INotificationService
{
    void SendNotification(string accountId, string message);
}