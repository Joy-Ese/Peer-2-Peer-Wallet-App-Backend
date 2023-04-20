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
            RuleFor(x => x.password).NotEmpty().Matches("^[a-zA-Z0-9]{6,}$")
                .WithMessage("Minimum six characters, at least one uppercase letter, one lowercase letter and one number");
            RuleFor(x => x.confirmPassword).NotEmpty().Matches("^[a-zA-Z0-9]{6,}$");
            RuleFor(x => x.email).EmailAddress().Matches("^[a-zA-Z0-9]+@[a-z]+.[a-z]{2,3}$")
                .WithMessage("Input correct email syntax");
            RuleFor(x => x.phoneNumber).Matches("^[0-9]{11}$")
                .WithMessage("Phone number required");
            RuleFor(x => x.firstName).NotEmpty().WithMessage("First name required").Length(3, 100);
            RuleFor(x => x.lastName).NotEmpty().WithMessage("Last name required").Length(3, 100);
            RuleFor(x => x.address).NotEmpty().WithMessage("Address required").Length(3, 100);

        }
    }
}
