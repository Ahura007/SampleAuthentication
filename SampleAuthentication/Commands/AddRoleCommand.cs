using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.Commands
{
	public class AddRoleCommand : ICommandBase
	{
		public string Name { get; set; }

		
		public void Validate()
		{
			if (string.IsNullOrEmpty(Name))
				throw new CommandValidationException("Name Should Not Be Empty .");
		}
	}
}