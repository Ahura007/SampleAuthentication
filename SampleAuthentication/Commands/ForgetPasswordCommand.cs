using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class ForgetPasswordCommand : ICommandBase
	{
		public string UserName { get; set; }

		
		public void Validate() => new ForgetPasswordCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class ForgetPasswordCommandValidator : AbstractValidator<ForgetPasswordCommand>
	{
		public ForgetPasswordCommandValidator()
		{
			RuleFor(p => p.UserName).NotEmpty();
		}
	}
}