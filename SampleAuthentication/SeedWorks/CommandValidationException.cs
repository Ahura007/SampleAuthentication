using System;

namespace SampleAuthentication.SeedWorks
{
	public class CommandValidationException : Exception
	{
		public CommandValidationException(string message)
			: base(message)
		{
		}
	}
}
