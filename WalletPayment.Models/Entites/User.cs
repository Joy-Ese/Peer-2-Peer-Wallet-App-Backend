using System.ComponentModel.DataAnnotations.Schema;

namespace WalletPayment.Models.Entites
{
    public class User
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Username { get; set; } = string.Empty;

        public byte[]? PasswordHash { get; set; }

        public byte[]? PasswordSalt { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Email { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string FirstName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string LastName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string Address { get; set; } = string.Empty;

        public byte[]? PinHash { get; set; }

        public byte[]? PinSalt { get; set; }

        public string? VerificationToken { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? PasswordResetToken { get; set; }

        public DateTime? ResetTokenExpiresAt { get; set; }

        public virtual Account UserAccount { get; set; }
        public virtual Image UserImage { get; set; }
        public virtual SecurityQuestion SecurityQuestions { get; set; }
        public virtual List<RefreshToken> RefreshTokens { get; set; }
        public virtual List<Deposit> Deposits { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
    }
}
