using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class UnAssignRoleCommand : ICommandBase
	{
		public string RoleId { get; set; }

		
		public void Validate() => new UnAssignRoleCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class UnAssignRoleCommandValidator : AbstractValidator<UnAssignRoleCommand>
	{
		public UnAssignRoleCommandValidator()
		{
			RuleFor(p => p.RoleId).NotEmpty();
		}
	}
}