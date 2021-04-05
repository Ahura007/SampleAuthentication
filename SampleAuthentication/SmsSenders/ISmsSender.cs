using System.Threading.Tasks;

namespace SampleAuthentication.SmsSenders
{
	public interface ISmsSender
	{
		Task SendSmsAsync(string message, string recipientMobileNumber);
	}
}