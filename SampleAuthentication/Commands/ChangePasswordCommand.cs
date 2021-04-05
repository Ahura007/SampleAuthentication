using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class ChangePasswordCommand : ICommandBase
	{
		public string CurrentPassword { get; set; }

		public string NewPassword { get; set; }

		public string ConfirmPassword { get; set; }

		public void Validate() => new ChangePasswordCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
	{
		public ChangePasswordCommandValidator()
		{
			RuleFor(p => p.CurrentPassword).NotEmpty();
			RuleFor(p => p.NewPassword).NotEmpty();
			RuleFor(p => p.ConfirmPassword).NotEmpty();
		}
	}
}
