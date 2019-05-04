using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repository;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var otp = Substitute.For<IOtp>();
            var hash = Substitute.For<IHash>();
            var notification = Substitute.For<INotification>();
            var logger = Substitute.For<ILogger>();
            var failedCounter = Substitute.For<IFailedCounter>();

            var authenticationService = new AuthenticationService(failedCounter,profile,hash,otp,logger,notification);

            otp.GetCurrentOtp("joey").ReturnsForAnyArgs("12345");
            profile.GetPassword("joey").ReturnsForAnyArgs("my hashed password");
            hash.GetHash("pw").ReturnsForAnyArgs("my hashed password");

            var isValid = authenticationService.Verify("joey", "pw", "12345");

            Assert.IsTrue(isValid);
        }
    }
}