using System;
using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
    public class RegisterCustomerCommand : ICommandBase
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string UserName { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

 
        
        public void Validate() => new RegisterCustomerCommandValidator().Validate(this).RaiseExceptionIfRequired();
    }

    public class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
    {
        public RegisterCustomerCommandValidator()
        {
            RuleFor(p => p.Name).NotEmpty().WithMessage("نام الزامی است");
            RuleFor(p => p.UserName).NotEmpty().WithMessage("نام کاربری الزامی است");
            RuleFor(p => p.PhoneNumber).NotEmpty().WithMessage("شماره موبایل الزامی است");
            RuleFor(p => p.Password).NotEmpty().WithMessage("کلمه عبور الزامی است");
        }
    }
}