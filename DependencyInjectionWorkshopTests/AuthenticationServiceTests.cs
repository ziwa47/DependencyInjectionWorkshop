using DependencyInjectionWorkshop.Models;

using NSubstitute;

using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtpService _otpService;
        private IProfileDao _profile;
        private AuthenticationService _sut;
        private string DefaultAccount = "joey";
        private string DefaultHashedPassword = "9487";
        private string DefaultOtp = "9527";
        private string DefaultPassword = "abc";

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _profile = Substitute.For<IProfileDao>();

            _sut =
                new AuthenticationService(_profile, _hash, _otpService, _failedCounter, _notification, _logger);
        }

        [Test]
        public void is_valid()
        {
            var isValid = WhenValid();
            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid_when_otp_is_wrong()
        {
            var isValid = WhenInIsValid();
            ShouldBeNotValid(isValid);
        }

        [Test]
        public void should_notify_when_invalid()
        {
           WhenInIsValid();
           _notification.Received()
               .PushMessage(Arg.Is<string>(m=>m.Contains(DefaultAccount)));
        }

        private bool WhenValid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultPassword);
            GivenHashedPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultOtp, DefaultAccount);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenInIsValid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultPassword);
            GivenHashedPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultOtp, DefaultAccount);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, "wrong otp");
            return isValid;
        }

        private void ShouldBeNotValid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private void ShouldBeValid(bool isValid)
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
            _profile.GetPassword(account).ReturnsForAnyArgs(passwordFromDb);
        }
    }
}