using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class VerifyVerificationCodeCommand : ICommandBase
	{
		public string Code { get; set; }

		
		public void Validate() => new VerifyVerificationCodeCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class VerifyVerificationCodeCommandValidator : AbstractValidator<VerifyVerificationCodeCommand>
	{
		public VerifyVerificationCodeCommandValidator()
		{
			RuleFor(p => p.Code).NotEmpty();
		}
	}
}