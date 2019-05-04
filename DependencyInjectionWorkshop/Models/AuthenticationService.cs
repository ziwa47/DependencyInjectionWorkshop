using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Exceptions;
using DependencyInjectionWorkshop.Repository;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator :IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        private bool CheckAccountIsLocked(string accountId)
        {
            return _failedCounter.CheckAccountIsLocked(accountId);
        }

        public bool Verify(string accountId, string password, string otp)
        {
            if (CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var isValid= _authenticationService.Verify(accountId, password, otp);
            if (isValid)
            {
                _failedCounter.Reset(accountId);
            }
            else
            {
                _failedCounter.Add(accountId);
            }

            return isValid;
        }
    }

    public class LogDecorator :IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogDecorator(IAuthentication authenticationService, IFailedCounter failedCounter, ILogger logger)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        private void LogVerify(string accountId)
        {
            var failedCount = _failedCounter.Get(accountId);
            _logger.Info($"accountId : {accountId}, failedTimes : {failedCount}");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogVerify(accountId);
            }

            return isValid;
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otp;

        public AuthenticationService(IProfile profile,
            IHash hash, IOtp otp)
        {
            _profile = profile;
            _hash = hash;
            _otp = otp;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var passwordFromDb = _profile.GetPassword(accountId);
            
            var hashPassword = _hash.GetHash(password);

            var currentOtp = _otp.GetCurrentOtp(accountId);

            if (string.Equals(currentOtp, otp) && string.Equals(passwordFromDb, hashPassword))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}