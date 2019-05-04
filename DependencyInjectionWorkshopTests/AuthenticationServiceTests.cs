using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repository;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "joey";
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultOtp = "12345";
        private const string DefaultPassword = "pw";
        private const int DefaultFailedCount = 91;
        private IAuthentication _authentication;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtp _otp;

        private IProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _otp = Substitute.For<IOtp>();
            _hash = Substitute.For<IHash>();
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();
            _failedCounter = Substitute.For<IFailedCounter>();


            var authenticationService =
                new AuthenticationService(_profile, _hash, _otp);

            var notificationDecorator = new NotificationDecorator(authenticationService,_notification);
            var failedCounterDecorator = new FailedCounterDecorator(notificationDecorator,_failedCounter);
            var logDecorator = new LogDecorator(failedCounterDecorator, _failedCounter, _logger);
            _authentication = logDecorator;
        }

        [Test]
        public void Account_Is_Locked()
        {
            //When void
            //_failedCounter
            //    .When(x=>x.CheckAccountIsLocked(DefaultAccountId))
            //    .Do(x=>throw new FailedTooManyTimesException());

            //When bool
            _failedCounter.CheckAccountIsLocked(DefaultAccountId).ReturnsForAnyArgs(true);

            TestDelegate action = () =>
            {
                _authentication.Verify(DefaultAccountId, DefaultHashedPassword, DefaultOtp);
            };
            Assert.Throws<FailedTooManyTimesException>(action);
        }

        [Test]
        public void Add_Failed_Count_When_Invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount();
        }

        [Test]
        public void Is_inValid_When_wrong_opt()
        {
            var isValid = WhenInvalid();
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void Is_Valid()
        {
            var isValid = WhenValid();
            ShouldBeValid(isValid);
        }

        [Test]
        public void Log_account_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);
            WhenInvalid();
            LogShouldContains(DefaultAccountId, DefaultFailedCount);
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotifyUser();
        }

        [Test]
        public void Reset_FailCount_When_Valid()
        {
            WhenValid();
            ShouldResetFailedCounter();
        }

        private void ShouldAddFailedCount()
        {
            _failedCounter.Received(1).Add(Arg.Any<string>());
        }

        private void ShouldResetFailedCounter()
        {
            _failedCounter.Received(1).Reset(Arg.Any<string>());
        }

        private void GivenFailedCount(int defaultFailedCount)
        {
            _failedCounter.Get(DefaultAccountId).ReturnsForAnyArgs(defaultFailedCount);
        }

        private void LogShouldContains(string defaultAccountId, int defaultFailedCount)
        {
            _logger.Received(1).Info(Arg.Is<string>(msg =>
                msg.Contains(defaultAccountId) && msg.Contains(defaultFailedCount.ToString())));
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);
            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultPassword, "wrong otp");
            return isValid;
        }

        private void ShouldNotifyUser()
        {
            _notification.Received(1).PushMessage(Arg.Any<string>());
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            return isValid;
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otp.GetCurrentOtp(accountId).ReturnsForAnyArgs(otp);
        }

        private void GivenHash(string password, string hashedPassword)
        {
            _hash.GetHash(password).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenPassword(string accountId, string password)
        {
            _profile.GetPassword(accountId).ReturnsForAnyArgs(password);
        }
    }
}