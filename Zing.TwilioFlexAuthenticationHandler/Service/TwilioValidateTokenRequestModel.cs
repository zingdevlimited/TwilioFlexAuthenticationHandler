using Newtonsoft.Json;

namespace Zing.TwilioFlexAuthenticationHandler.Service
{
    public class TwilioValidateTokenRequestModel
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
