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
    }
}
