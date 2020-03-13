using Microsoft.AspNetCore.Authentication;
using System;

namespace Zing.TwilioFlexAuthenticationHandler
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding Twilio Flex authentication handler into the authentication chain
        /// </summary>
        /// <param name="authenticationScheme"></param>
        /// <param name="builder"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddTwilioFlex(
            this AuthenticationBuilder builder,
            string authenticationScheme,
            Action<TwilioFlexAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<TwilioFlexAuthenticationOptions, TwilioFlexAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}
