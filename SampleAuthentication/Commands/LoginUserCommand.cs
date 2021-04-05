using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class LoginUserCommand : ICommandBase
	{
        public string UserName { get; set; }

        public string Password { get; set; }

        public string CaptchaId { get; set; }

        public string CaptchaCode { get; set; }

		
		public void Validate() => new LoginUserCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
	{
        public LoginUserCommandValidator()
        {
            RuleFor(p => p.UserName).NotEmpty().WithMessage("نام کاربری یا رمز عبور صحیح نمی باشد");
            RuleFor(p => p.Password).NotEmpty().WithMessage("نام کاربری یا رمز عبور صحیح نمی باشد");
            //RuleFor(p => p.CaptchaId).NotEmpty().WithMessage("کد امنیتی نامعتبر می باشد");
            //RuleFor(p => p.CaptchaCode).NotEmpty().WithMessage("کد امنیتی صحیح نمی باشد");
        }
	}
}
