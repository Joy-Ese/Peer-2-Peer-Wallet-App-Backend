using WalletPayment.Models.DataObjects;

namespace WalletPayment.Services.Interfaces
{
    public interface INotification
    {
        Task SendNotification(string user, string message);
        Task SendTransferNotification(int recieverId, string currency, string amount, string sender);
        Task<List<GetNotificationModel>> GetAllNotifications();
        Task<List<GetUnreadNotificationModel>> GetAllUnreadNotifications();
        Task<GetAllUnreadNotificationsNoModel> GetAllUnreadNotificationsNo();
        Task<bool> SetNotificationsToRead(GetUnreadNotificationDTO req);
    }
}
