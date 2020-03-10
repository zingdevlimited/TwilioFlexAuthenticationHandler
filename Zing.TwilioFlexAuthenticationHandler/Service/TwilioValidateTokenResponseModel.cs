using Newtonsoft.Json;
using System;

namespace Zing.TwilioFlexAuthenticationHandler.Service
{
    public class TwilioValidateTokenResponseModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("worker_sid")]
        public string WorkerSID { get; set; }

        [JsonProperty("roles")]
        public string[] Roles { get; set; }

        [JsonProperty("realm_user_id")]
        public string Email { get; set; }

        [JsonProperty("valid")]
        public bool IsValid { get; set; }

        [JsonProperty("expiration")]
        public DateTime? ExpiresAtUtc { get; set; }

        [JsonProperty("message")]
        public object ErrorMessage { get; set; }

        [JsonProperty("identity")]
        public string Identity { get; set; }
    }
}
