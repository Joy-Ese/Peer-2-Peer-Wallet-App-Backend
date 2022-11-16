﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WalletPayment.Models.DataObjects
{
    public class UserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string ConfirmPin { get; set; } = string.Empty;
    }

    public class UserDashboardViewModel
    {
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public string Balance { get; set; } = string.Empty;
    }

    public class UserSignUpDto
    {
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phoneNumber { get; set; } = string.Empty;
        public string firstName { get; set; } = string.Empty;
        public string lastName { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^[0-9]{4}$", ErrorMessage = "Pin must not be more than 4 digits")]
        public string pin { get; set; } = string.Empty;
        [Required]

        [Compare("pin", ErrorMessage = "Pins do not match")]
        public string confirmPin { get; set; } = string.Empty;

    }

    public class UserLoginDto
    {
        [Required]
        public string username { get; set; } = string.Empty;
        [Required]
        public string password { get; set; } = string.Empty;
    }

    public class AccountViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public class TransactionDto
    {
        public string sourceAccount { get; set; } = string.Empty;
        public string destinationAccount { get; set; } = string.Empty;
        public decimal amount { get; set; } 
    }


    public class CustomClaims
    {
        public const string UserId = "http://schemas.microsoft.com/ws/2008/06/identity/claims/userid";
        public const string UserName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/username";
        public const string AccountNumber = "http://schemas.microsoft.com/ws/2008/06/identity/claims/accountnumber";
        public const string FirstName = "http://schemas.microsoft.com/ws/2008/06/identity/claims/firstname";
        public const string Balance = "http://schemas.microsoft.com/ws/2008/06/identity/claims/balance";
    }

   
}
