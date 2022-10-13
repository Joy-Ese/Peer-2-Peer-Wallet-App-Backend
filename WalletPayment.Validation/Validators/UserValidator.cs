using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletPayment.Models.DataObjects;

namespace WalletPayment.Validation.Validators
{
    public class UserValidator : AbstractValidator<UserSignUpDto>
    {
        public UserValidator()
        {
            RuleFor(x => x.username).NotEmpty().WithMessage("Username required").Length(3, 100);
            RuleFor(x => x.password).NotEmpty().Matches("(?=.*[a-z])(?=.*[A-Z])(?=.*d)[a-zA-Zd]{8,}$")
                .WithMessage("Minimum eight characters, at least one uppercase letter, one lowercase letter and one number");
            RuleFor(x => x.email).EmailAddress().Matches("^[a-z0-9]+@[a-z]+.[a-z]{2,3}$")
                .WithMessage("Input correct email syntax");
            RuleFor(x => x.phoneNumber).Matches("^[0][1-9]d{9}$|^[1-9]d{9}$")
                .WithMessage("Phone number required");
            RuleFor(x => x.firstName).NotEmpty().WithMessage("First name required").Length(3, 100);
            RuleFor(x => x.lastName).NotEmpty().WithMessage("Last name required").Length(3, 100);
            RuleFor(x => x.address).NotEmpty().WithMessage("Address required").Length(3, 100);
            
        }
    }
}
