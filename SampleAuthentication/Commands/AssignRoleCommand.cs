using FluentValidation;
using SampleAuthentication.Extensions;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class AssignRoleCommand : ICommandBase
	{
		public string[] RoleIds { get; set; }

		
		public void Validate() => new AssignRoleCommandValidator().Validate(this).RaiseExceptionIfRequired();
	}

	public class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
	{
		public AssignRoleCommandValidator()
		{
			RuleFor(p => p.RoleIds).NotEmpty();
		}
	}
}