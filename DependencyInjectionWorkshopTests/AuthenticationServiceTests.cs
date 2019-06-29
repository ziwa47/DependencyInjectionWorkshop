using System;
using DependencyInjectionWorkshop.Models;

using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const int DefaultFailedCount = 91;
        private const string DefaultAccount = "joey";
        private const string DefaultHashedPassword = "9487";
        private const string DefaultOtp = "9527";
        private const string DefaultPassword = "abc";
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtpService _otpService;
        private IProfileDao _profile;
        private IAuthenticationService _sut;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _profile = Substitute.For<IProfileDao>();

            var authenticationService = new AuthenticationService(_failedCounter, _logger, _otpService, _profile, _hash);
            var accountLockDecorator = new IsAccountLockDecorator(authenticationService,_failedCounter);
            _sut = new NotificationDecorator(accountLockDecorator, _notification);

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
            var isValid = WhenIsInValid();
            ShouldBeNotValid(isValid);
        }

        [Test]
        public void should_notify_when_invalid()
        {
            WhenIsInValid();
            ShouldNotify(DefaultAccount);
        }

        [Test]
        public void should_add_failedCount_when_invalid()
        {
            WhenIsInValid();
            ShouldAddFailedCount(DefaultAccount);
        }

        [Test]
        public void should_reset_failedCount_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount(DefaultAccount);
        }

        [Test]
        public void should_log_failed_count_where_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenIsInValid();
            ShouldLog(DefaultAccount, DefaultFailedCount);
        }

        [Test]
        public void account_is_lock()
        {
            GivenAccountLocked();
            ShouldThrow<FailedTooManyTimesException>(() => WhenValid());
        }

        private void GivenAccountLocked()
        {
            _failedCounter.IsAccountLocked(DefaultAccount).ReturnsForAnyArgs(true);
        }

        private void ShouldThrow<T>(TestDelegate testDelegate) where T : Exception
        {
            Assert.Throws<T>(testDelegate);
        }

        private void ShouldLog(string account, int failedCount)
        {
            _logger.Received().Info(Arg.Is<string>(m => m.Contains(account) && m.Contains(failedCount.ToString())));
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.GetFailedCount(DefaultAccount).ReturnsForAnyArgs(failedCount);
        }

        private void ShouldResetFailedCount(string account)
        {
            _failedCounter.Received().ResetFailedCount(account);
        }

        private void ShouldAddFailedCount(string account)
        {
            _failedCounter.Received().AddFailedCount(account);
        }

        private void ShouldNotify(string account)
        {
            _notification.Received()
                .PushMessage(Arg.Is<string>(m => m.Contains(account)));
        }

        private bool WhenValid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashedPassword);
            GivenHashedPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenVerify(DefaultAccount, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenIsInValid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashedPassword);
            GivenHashedPassword(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

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

        private void GivenOtp(string account, string otp)
        {
            _otpService.GetOtpResp(account).ReturnsForAnyArgs(otp);
        }

        private void GivenHashedPassword(string plainText, string hashedPassword)
        {
            _hash.Hash(plainText).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenPasswordFromDb(string account, string passwordFromDb)
        {
            _profile.GetPassword(account).ReturnsForAnyArgs(passwordFromDb);
        }
    }
}