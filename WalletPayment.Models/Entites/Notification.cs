using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class Notification
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsNotificationRead { get; set; }
        public DateTime Date { get; set; }
        public int NotificationUserId { get; set; }

        [ForeignKey("NotificationUserId")]
        public virtual User NotificationUser { get; set; }


        public Notification()
        {
            Reference = $"{Guid.NewGuid().ToString().Replace("-", "").Substring(1, 15)}";
        }
    }
}
