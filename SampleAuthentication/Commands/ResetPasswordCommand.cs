using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class ResetPasswordCommand : ICommandBase
	{
		/// <summary>
		/// کد
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// کلمه عبور جدید
		/// </summary>
		public string NewPassword { get; set; }

		
		public void Validate() => new ResetPasswordCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
	{
		public ResetPasswordCommandValidator()
		{
			RuleFor(p => p.Code).NotEmpty();
			RuleFor(p => p.NewPassword).NotEmpty();
		}
	}
}