using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.Entites
{
    public class Admins
    {
        public int Id { get; set; }
        public bool IsSecure { get; set; }
        public bool IsDisabled { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Username { get; set; } = string.Empty;

        [Column(TypeName = "varchar(20)")]
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public byte[]? PasswordHash { get; set; }

        public byte[]? PasswordSalt { get; set; }
    }
}
