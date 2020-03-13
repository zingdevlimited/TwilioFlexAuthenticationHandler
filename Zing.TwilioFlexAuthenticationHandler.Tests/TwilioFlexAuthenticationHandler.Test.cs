using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Zing.TwilioFlexAuthenticationHandler;
using static Zing.TwilioFlexAuthenticationHandler.Constants;

namespace TestTwilioFlexAuthenticationHandler
{
    [TestClass]
    public class TwilioFlexAuthenticationHandlerTest
    {
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<UrlEncoder> _encoder;
        private readonly Mock<ISystemClock> _clock;

        public TwilioFlexAuthenticationHandlerTest()
        {
            var logger = new Mock<ILogger<TwilioFlexAuthenticationHandler>>();
            _loggerFactory = new Mock<ILoggerFactory>();
            _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(logger.Object);

            _encoder = new Mock<UrlEncoder>();
            _clock = new Mock<ISystemClock>();
        }

        [TestMethod]
        public async Task HandleAuthenticateAsyncThrowsIfAccountSIDNotSet() {
            var context = new DefaultHttpContext();
            var handler = SetupHandler(new TwilioFlexAuthenticationOptions());

            await handler.InitializeAsync(new AuthenticationScheme("Bearer", null, typeof(TwilioFlexAuthenticationHandler)), context);
            try
            {
                await handler.AuthenticateAsync();
            } 
            catch (ArgumentException ex)
            {
                ex.Message.Should().Be("Options missing AccountSID");
            }
            catch (Exception ex)
            {
                throw new AssertFailedException("Expected exception of type 'ArgumentException' but got 'Exception'", ex);
            }
        }

        [TestMethod]
        public async Task HandleAuthenticateAsyncThrowsIfAuthTokenNotSet()
        {
            var context = new DefaultHttpContext();
            var handler = SetupHandler(new TwilioFlexAuthenticationOptions { AccountSID = "12345" });

            await handler.InitializeAsync(new AuthenticationScheme("Bearer", null, typeof(TwilioFlexAuthenticationHandler)), context);
            try
            {
                await handler.AuthenticateAsync();
            }
            catch (ArgumentException ex)
            {
                ex.Message.Should().Be("Options missing AuthToken");
            }
            catch (Exception ex)
            {
                throw new AssertFailedException("Expected exception of type 'ArgumentException' but got 'Exception'", ex);
            }
        }

        [TestMethod]
        public async Task HandleAuthenticateAsyncReturnsNoResultIfAuthorizationMissing()
        {
            var context = new DefaultHttpContext();
            var handler = SetupHandler(new TwilioFlexAuthenticationOptions { AccountSID = "12345", AuthToken = "Testing" });

            await handler.InitializeAsync(new AuthenticationScheme("Bearer", null, typeof(TwilioFlexAuthenticationHandler)), context);

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        public async Task HandleAuthenticateAsyncReturnsNoResultIfAuthorizationTokenPrefixIsWrong()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "12345");

            var handler = SetupHandler(new TwilioFlexAuthenticationOptions { AccountSID = "12345", AuthToken = "Testing" });

            await handler.InitializeAsync(new AuthenticationScheme("Bearer", null, typeof(TwilioFlexAuthenticationHandler)), context);

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeFalse();
        }

        [TestMethod]
        public async Task HandleAuthenticateAsyncReturnsFromCacheWhenAvailable()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Add(HeaderNames.Authorization, "Bearer 12345");

            var cache = new Dictionary<string, KeyValuePair<string, string>[]>();
            cache.Add("flex:token:12345", new KeyValuePair<string, string>[] {
                new KeyValuePair<string, string>(TwilioFlex.ClaimTypes.Email, "test@email.com"),
                new KeyValuePair<string, string>(TwilioFlex.ClaimTypes.WorkerSID, "WSID12345")
            });
            var handler = SetupHandler(new TwilioFlexAuthenticationOptions { AccountSID = "12345", AuthToken = "Testing" }, cache);

            await handler.InitializeAsync(new AuthenticationScheme("Bearer", null, typeof(TwilioFlexAuthenticationHandler)), context);

            var result = await handler.AuthenticateAsync();

            result.Succeeded.Should().BeTrue();
            result.Principal.Claims.Should().Contain(x => x.Type == TwilioFlex.ClaimTypes.Email && x.Value == "test@email.com");
            result.Principal.Claims.Should().Contain(x => x.Type == TwilioFlex.ClaimTypes.WorkerSID && x.Value == "WSID12345");
        }

        private TwilioFlexAuthenticationHandler SetupHandler(TwilioFlexAuthenticationOptions opts, Dictionary<string, KeyValuePair<string, string>[]> cacheValues = null)
        {
            var options = new Mock<IOptionsMonitor<TwilioFlexAuthenticationOptions>>();
            options.Setup(x => x.Get("Bearer")).Returns(opts);

            var services = new ServiceCollection();
            services.AddMemoryCache();
            var provider = services.BuildServiceProvider();

            var memoryCache = provider.GetService<IMemoryCache>();

            if (cacheValues != null)
            {
                foreach (var cacheValue in cacheValues)
                {
                    memoryCache.Set(cacheValue.Key, cacheValue.Value);
                }
            }

            return new TwilioFlexAuthenticationHandler(options.Object, _loggerFactory.Object, _encoder.Object, _clock.Object, memoryCache);
        }
    }
}
