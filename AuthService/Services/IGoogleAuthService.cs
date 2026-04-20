using Google.Apis.Auth;

namespace AuthService.Services
{
    public interface IGoogleAuthService
    {
        Task<GoogleJsonWebSignature.Payload> ValidateIdToken(string idToken);
    }
}
