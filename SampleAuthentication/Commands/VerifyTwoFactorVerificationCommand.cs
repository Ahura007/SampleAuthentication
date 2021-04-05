using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
    public class VerifyTwoFactorVerificationCommand : ICommandBase
    {
        public string VerifyCode { get; set; }
        
        public void Validate() => new VerifyTwoFactorVerificationCommandValidator().Validate(this).RaiseExceptionIfRequired();
    }

    public class VerifyTwoFactorVerificationCommandValidator : AbstractValidator<VerifyTwoFactorVerificationCommand>
    {
        public VerifyTwoFactorVerificationCommandValidator()
        {
            RuleFor(p => p.VerifyCode).NotEmpty().WithMessage("کد تایید الزامی است");
            RuleFor(p => p.VerifyCode).NotNull().WithMessage("کد تایید الزامی است");
            RuleFor(p => p.VerifyCode).Length(6).WithMessage("کد تایید 6 کارکتر باید باشد");
        }
    }
}
