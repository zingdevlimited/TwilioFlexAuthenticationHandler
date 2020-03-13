using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Zing.TwilioFlexAuthenticationHandler.Service;
using static Zing.TwilioFlexAuthenticationHandler.Constants;

[assembly: InternalsVisibleTo("TestTwilioFlexAuthenticationHandler")]
namespace Zing.TwilioFlexAuthenticationHandler
{
    /// <summary>
    /// An implementation of AuthenticationHandler intended to manage authentication via a Twilio Flex JWT
    /// </summary>
    
    public class TwilioFlexAuthenticationHandler : AuthenticationHandler<TwilioFlexAuthenticationOptions>
    {
        private const int MAX_CACHE_TIME_IN_MINUTES = 15;

        private readonly IMemoryCache cache;

        /// <summary>
        /// Constructor returning an instance of TwilioFlexAuthenticationHandler
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="encoder"></param>
        /// <param name="clock"></param>
        /// <param name="cache"></param>
        public TwilioFlexAuthenticationHandler(
            IOptionsMonitor<TwilioFlexAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IMemoryCache cache) : base(options, logger, encoder, clock)
        {
            this.cache = cache;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (string.IsNullOrWhiteSpace(Options.AccountSID)) throw new ArgumentException("Options missing AccountSID", Options.AccountSID);
            if (string.IsNullOrWhiteSpace(Options.AuthToken)) throw new ArgumentException("Options missing AuthToken", Options.AuthToken);

            if (Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeaders))
            {
                const string cachePrefix = "flex:token:";

                string tokenPrefix = Options.TokenPrefix != null ? $"{Options.TokenPrefix} " : "Bearer ";
                var authHeader = authHeaders.FirstOrDefault(e => e.StartsWith(tokenPrefix, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrWhiteSpace(authHeader))
                {
                    //have token, trim off prefix
                    var accessToken = authHeader.Substring(tokenPrefix.Length);

                    //check for cached value
                    KeyValuePair<string, string>[] cachedClaims;
                    if (cache.TryGetValue($"{cachePrefix}{accessToken}", out cachedClaims) && cachedClaims != null && cachedClaims.Length > 0)
                    {
                        //found matching values in cache, use them and return now.
                        return GenerateSuccessResult(cachedClaims);
                    }

                    //no values in cache, call API to validate token
                    var introspectResult = await TwilioIdentityApiService.ValidateTokenAsync(accessToken, Options.AccountSID, Options.AuthToken);
                    if (introspectResult != null && introspectResult.IsValid && introspectResult.ExpiresAtUtc.HasValue && introspectResult.ExpiresAtUtc.Value > DateTime.UtcNow)
                    {
                        //generate claims
                        var claimsToCache = new List<KeyValuePair<string, string>>();
                        claimsToCache.Add(new KeyValuePair<string, string>(TwilioFlex.ClaimTypes.Email, introspectResult.Email));
                        claimsToCache.Add(new KeyValuePair<string, string>(TwilioFlex.ClaimTypes.WorkerSID, introspectResult.WorkerSID));

                        if (introspectResult.Roles != null && introspectResult.Roles.Length > 0)
                        {
                            foreach (var role in introspectResult.Roles)
                            {
                                claimsToCache.Add(new KeyValuePair<string, string>(TwilioFlex.ClaimTypes.WorkerRole, role));
                            }
                        }

                        //add to cache to save network call of every request. set expiry to max or actual session expiry, whatever is the soonest.
                        //setting a short cache time so if token is revoked then this will be the max time until
                        //all local caches are cleared and access is guaranteed to be blocked.
                        var claimsArray = claimsToCache.ToArray();
                        var cacheExpiry = DateTimeOffset.UtcNow.AddMinutes(MAX_CACHE_TIME_IN_MINUTES);
                        if (cacheExpiry > introspectResult.ExpiresAtUtc.Value)
                        {
                            cacheExpiry = introspectResult.ExpiresAtUtc.Value;
                        }
                        cache.Set($"{cachePrefix}{accessToken}", claimsArray, cacheExpiry);
                        return GenerateSuccessResult(claimsArray);
                    }
                }
            }
            //if control gets here did not authenticate successfully
            //return NoResult so if any other authentication handlers are registered they can try.
            return AuthenticateResult.NoResult();
        }

        private AuthenticateResult GenerateSuccessResult(KeyValuePair<string, string>[] inputClaims)
        {
            var claims = new List<Claim>();
            for (int i = 0, l = inputClaims.Length; i < l; i++)
            {
                if (inputClaims[i].Value != null)
                    claims.Add(new Claim(inputClaims[i].Key, inputClaims[i].Value));
            }

            //NOTE: using Twilio worker roles as the role claim.
            //this will allow you to easily do role based authorisation.
            var identities = new List<ClaimsIdentity> { new ClaimsIdentity(claims, Scheme.Name, TwilioFlex.ClaimTypes.Email, TwilioFlex.ClaimTypes.WorkerRole) };
            return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identities), Scheme.Name));
        }
    }
}
