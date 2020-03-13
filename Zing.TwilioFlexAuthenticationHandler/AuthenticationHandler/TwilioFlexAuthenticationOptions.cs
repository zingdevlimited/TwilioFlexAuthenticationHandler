using Microsoft.AspNetCore.Authentication;

namespace Zing.TwilioFlexAuthenticationHandler
{
    public class TwilioFlexAuthenticationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// The expected prefix before the token in the Authorization header
        /// E.g. 'Bearer'
        /// </summary>
        public string TokenPrefix { get; set; }

        /// <summary>
        /// The account SID for the Twilio project you are handling Flex tokens from
        /// </summary>
        public string AccountSID { get; set; }

        /// <summary>
        /// The auth token for the Twilio project you are handling Flex tokens from
        /// </summary>
        public string AuthToken { get; set; }
    }
}
