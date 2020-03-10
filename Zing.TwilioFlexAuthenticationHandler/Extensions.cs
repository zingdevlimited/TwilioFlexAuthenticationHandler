using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Zing.TwilioFlexAuthenticationHandler.Service;

namespace Zing.TwilioFlexAuthenticationHandler
{
    public static class Extensions
    {
        /// <summary>
        /// Extension method for adding Twilio Flex authentication handler into the authentication chain
        /// </summary>
        /// <param name="services"></param>
        /// <param name="authenticationScheme"></param>
        /// <param name="configureOptions"></param>
        /// <param name="configuration"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static AuthenticationBuilder AddTwilioFlex(
            this IServiceCollection services,
            string authenticationScheme,
            Action<TwilioFlexAuthenticationOptions> configureOptions,
            IConfiguration configuration,
            AuthenticationBuilder builder = null)
        {
            var twilioSettingsSection = configuration.GetSection(nameof(TwilioSettings));
            if (!twilioSettingsSection.Exists()) throw new ArgumentNullException(nameof(configuration), $"Missing section '{nameof(TwilioSettings)}' in configuration.");
            var twilioSettings = twilioSettingsSection.Get<TwilioSettings>();
            if (string.IsNullOrWhiteSpace(twilioSettings.AccountSID))
                throw new ArgumentNullException(nameof(configuration), $"Missing property '{nameof(twilioSettings.AccountSID)}' in configuration section '{nameof(TwilioSettings)}'.");
            if (string.IsNullOrWhiteSpace(twilioSettings.AuthToken))
                throw new ArgumentNullException(nameof(configuration), $"Missing property '{nameof(twilioSettings.AuthToken)}' in configuration section '{nameof(TwilioSettings)}'.");

            services.Configure<TwilioSettings>(options => {
                options.AccountSID = twilioSettings.AccountSID;
                options.AuthToken = twilioSettings.AuthToken;
            });

            services.AddHttpClient<ITwilioIdentityApiService, TwilioIdentityApiService>();

            var internalBuilder = builder ?? services.AddAuthentication();
            return internalBuilder.AddScheme<TwilioFlexAuthenticationOptions, TwilioFlexAuthenticationHandler>(authenticationScheme, configureOptions);
        }
    }
}
