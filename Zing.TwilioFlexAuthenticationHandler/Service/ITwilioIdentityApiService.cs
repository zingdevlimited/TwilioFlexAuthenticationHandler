using System.Threading.Tasks;

namespace Zing.TwilioFlexAuthenticationHandler.Service
{
    public interface ITwilioIdentityApiService
    {
        Task<TwilioValidateTokenResponseModel> ValidateTokenAsync(string token);
    }
}
