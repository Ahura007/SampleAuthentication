namespace SampleAuthentication.SeedWorks
{
	public static class CommandValidator
	{
		public static void Validate(ICommandBase command)
		{
			if (command is null)
				throw new CommandValidationException("Command Should Not Be Empty .");

			command.Validate();
		}
	}
}