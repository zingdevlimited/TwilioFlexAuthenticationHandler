using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Zing.TwilioFlexAuthenticationHandler;
using Zing.TwilioFlexAuthenticationHandler.Service;

namespace TestTwilioFlexAuthenticationHandler
{
    [TestClass]
    public class ExtensionsTest
    {
        private Mock<IServiceCollection> services;
        private IConfiguration configuration;

        [TestInitialize]
        public void Setup()
        {
            services = new Mock<IServiceCollection>();
        }

        [TestMethod]
        public void AddTwilioFlexThrowsArgumentExceptionForMissingTwilioSection()
        {
            var configuration = new Mock<IConfiguration>().Object;

            Assert.ThrowsException<ArgumentNullException>(() => services.Object.AddTwilioFlex("Bearer", options => { }, configuration), $"Missing section '{nameof(TwilioSettings)}' in configuration.");
        }

        [TestMethod]
        public void AddTwilioFlexThrowsArgumentExceptionForMissingTwilioSettings()
        {
            Assert.ThrowsException<ArgumentNullException>(() => services.Object.AddTwilioFlex("Bearer", options => { }, configuration), $"Missing section '{nameof(TwilioSettings)}' in configuration.");
        }
    }
}
