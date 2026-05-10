using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NotificationService.Services
{
    public class TwilioSmsSender : ISmsSender
    {
        private readonly IConfiguration _configuration;

        public TwilioSmsSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendSms(string phoneNumber, string body)
        {
            var sid = _configuration["Twilio:AccountSid"];
            var token = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:FromNumber"];

            if (string.IsNullOrWhiteSpace(sid)
                || string.IsNullOrWhiteSpace(token)
                || string.IsNullOrWhiteSpace(fromNumber))
            {
                throw new InvalidOperationException("Twilio configuration is incomplete.");
            }

            TwilioClient.Init(sid, token);

            await MessageResource.CreateAsync(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(fromNumber),
                body: body);
        }
    }
}
