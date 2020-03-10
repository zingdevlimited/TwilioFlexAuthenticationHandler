using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Zing.TwilioFlexAuthenticationHandler.Service
{
    public class TwilioIdentityApiService : ITwilioIdentityApiService
    {
        private readonly HttpClient client;
        private readonly Uri validateTokenUri;
        private readonly string authHeaderValue;

        /// <summary>
        /// Constructor returning instance of TwilioIdentityApiService
        /// </summary>
        /// <param name="client"></param>
        /// <param name="config"></param>
        public TwilioIdentityApiService(HttpClient client, IOptions<TwilioSettings> config)
        {
            if (client == null) { throw new ArgumentNullException(nameof(client)); }
            if (string.IsNullOrWhiteSpace(config.Value.AccountSID)) { throw new ArgumentException($"{nameof(config.Value.AccountSID)} property is missing", nameof(config)); }
            if (string.IsNullOrWhiteSpace(config.Value.AuthToken)) { throw new ArgumentException($"{nameof(config.Value.AuthToken)} property is missing", nameof(config)); }

            this.client = client;
            if (!Uri.TryCreate($"https://iam.twilio.com/v1/Accounts/{config.Value.AccountSID}/Tokens/validate", UriKind.Absolute, out validateTokenUri) || validateTokenUri == null)
            {
                throw new ArgumentException($"{nameof(config.Value.AccountSID)} value parsed in does not merge to a valid Uri", nameof(config));
            }
            byte[] data = Encoding.UTF8.GetBytes($"{config.Value.AccountSID}:{config.Value.AuthToken}");
            authHeaderValue = $"Basic {Convert.ToBase64String(data)}";
        }

        /// <summary>
        /// Introspects the given token and returns the result.
        /// </summary>
        /// <param name="token">token to introspect.</param>
        /// <returns>Introspect result.</returns>
        public async Task<TwilioValidateTokenResponseModel> ValidateTokenAsync(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                using (var req = new HttpRequestMessage(HttpMethod.Post, validateTokenUri))
                {
                    req.Content = new StringContent(JsonConvert.SerializeObject(new TwilioValidateTokenRequestModel() { Token = token }), Encoding.UTF8, "application/json");
                    req.Headers.Add("Authorization", authHeaderValue);
                    using (var res = await client.SendAsync(req))
                    {
                        if (res.IsSuccessStatusCode)
                        {
                            var resContent = await res.Content.ReadAsStringAsync();
                            var introspectResult = JsonConvert.DeserializeObject<TwilioValidateTokenResponseModel>(resContent);
                            return introspectResult;
                        }
                        else
                        {
                            return new TwilioValidateTokenResponseModel() { IsValid = false, ErrorMessage = $"Unexpected Response {res.StatusCode}" };
                        }
                    }
                }
            }
            return new TwilioValidateTokenResponseModel() { IsValid = false, ErrorMessage = "Unknown Error" };
        }
    }
}
