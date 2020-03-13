using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Zing.TwilioFlexAuthenticationHandler.Service
{
    public class TwilioIdentityApiService
    {
        /// <summary>
        /// Introspects the given token and returns the result.
        /// </summary>
        /// <param name="token">token to introspect.</param>
        /// <returns>Introspect result.</returns>
        public static async Task<TwilioValidateTokenResponseModel> ValidateTokenAsync(string token, string accountSID, string authToken)
        {
            if (!Uri.TryCreate($"https://iam.twilio.com/v1/Accounts/{accountSID}/Tokens/validate", UriKind.Absolute, out var validateTokenUri) || validateTokenUri == null)
            {
                throw new ArgumentException($"{nameof(accountSID)} value parsed in does not merge to a valid Uri");
            }

            byte[] data = Encoding.UTF8.GetBytes($"{accountSID}:{authToken}");
            var authHeaderValue = $"Basic {Convert.ToBase64String(data)}";

            if (!string.IsNullOrWhiteSpace(token))
            {
                using (var req = new HttpRequestMessage(HttpMethod.Post, validateTokenUri))
                {
                    req.Content = new StringContent(JsonConvert.SerializeObject(new TwilioValidateTokenRequestModel() { Token = token }), Encoding.UTF8, "application/json");
                    req.Headers.Add("Authorization", authHeaderValue);

                    using (var client = new HttpClient())
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
