using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Zing.TwilioFlexAuthenticationHandler;

namespace TestTwilioFlexAuthenticationHandler
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void AddTwilioFlexConfiguresAuthenticationScheme()
        {
            var serviceCollection = new ServiceCollection();
            var authBuilder = new Mock<AuthenticationBuilder>(serviceCollection);
            authBuilder
                .Setup(b => b.AddScheme<TwilioFlexAuthenticationOptions, TwilioFlexAuthenticationHandler>(
                    It.IsAny<string>(),
                    It.IsAny<Action<TwilioFlexAuthenticationOptions>>()))
                .Returns<AuthenticationBuilder>(null)
                .Verifiable();

            authBuilder.Object
                .AddTwilioFlex("Bearer", options =>
                {
                    options.TokenPrefix = "Bearer";
                    options.AccountSID = "12345";
                    options.AuthToken = "Testing";
                });

            authBuilder.Verify();
        }
    }
}
