using Google.Apis.Auth;

namespace AuthService.Services
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleJsonWebSignature.Payload> ValidateIdToken(string idToken)
        {
            var clientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new InvalidOperationException("GoogleAuth:ClientId is missing in configuration.");
            }

            return await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [clientId]
            });
        }
    }
}
