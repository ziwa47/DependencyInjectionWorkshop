using DependencyInjectionWorkshop.Models;

using NSubstitute;

using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private readonly INotification _notification = Substitute.For<INotification>();
        private readonly IProfileDao _profileDao = Substitute.For<IProfileDao>();
        private readonly IHash _hash = Substitute.For<IHash>();
        private readonly IOtpService _otpService = Substitute.For<IOtpService>();
        private readonly IFailedCounter _failedCounter = Substitute.For<IFailedCounter>();
        private readonly ILogger _logger = Substitute.For<ILogger>();
        private readonly AuthenticationService _sut;
        private string DefaultAccount = "joey";
        private string DefaultPassword = "abc";
        private string DefaultHashedPassword = "9487";
        private string DefaultOtp = "9527";

        public AuthenticationServiceTests()
        {
            _sut = new AuthenticationService(_profileDao, _hash, _otpService, _failedCounter, _notification, _logger);
        }

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultPassword);
            GivenHashedPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultOtp, DefaultAccount);

            var isValid = WhenVerify(DefaultAccount, DefaultHashedPassword, DefaultOtp);
            ShouldValid(isValid);
        }

        private static void ShouldValid(bool isValid)
        {
            Assert.True(isValid);
        }

        private bool WhenVerify(string account, string password, string otp)
        {
            var isValid = _sut.Verify(account, password, otp);
            return isValid;
        }

        private void GivenOtp(string otp, string account)
        {
            _otpService.GetOtpResp(account).ReturnsForAnyArgs(otp);
        }

        private void GivenHashedPassword(string hashedPassword, string plainText)
        {
            _hash.Hash(plainText).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenPasswordFromDb(string account, string passwordFromDb)
        {
            _profileDao.GetPassword(account).ReturnsForAnyArgs(passwordFromDb);
        }
    }
}