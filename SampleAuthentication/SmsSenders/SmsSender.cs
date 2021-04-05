using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SampleAuthentication.Activators;
using SampleAuthentication.SeedWorks;

namespace SampleAuthentication.SmsSenders
{
    public class SmsSender : ISmsSender
    {
        private readonly IHttpClientHelper _clientHelper;
        private readonly IOptions<HostAddresses> _hostAddress;

        public SmsSender(IHttpClientHelper clientHelper, IOptions<HostAddresses> hostAddress)
        {
            _clientHelper = clientHelper;
            _hostAddress = hostAddress;
        }

        
        public async Task SendSmsAsync(string message, string receiverPhoneNumber)
        {
            try
            {
                var sendSmsCommand = new
                {
                    Message = message,
                    MicroServiceName = "Authentication",
                    MobileNumber = receiverPhoneNumber
                };

                await _clientHelper.PostAsync<object, object>(_hostAddress.Value.NotificationAddress + "/sms", sendSmsCommand);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sms Send Log : " + e.Message);
            }
        }
    }
}
