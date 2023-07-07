namespace WalletPayment.Services.Interfaces
{
    public interface INotification
    {
        Task SendNotification(string user, string message);
        Task SendTransferNotification(int recieverId, string currency, string amount, string sender);
        //Get all notifications & Get all unread notifications
    }
}
