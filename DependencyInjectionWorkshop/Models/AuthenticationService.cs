using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Repository;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly ILogger _logger;
        private readonly INotification _notification;

        public AuthenticationService(IFailedCounter failedCounter, IProfile profile,
            IHash hash, IOtp otp, ILogger logger, INotification notification)
        {
            _failedCounter = failedCounter;
            _profile = profile;
            _hash = hash;
            _otp = otp;
            _logger = logger;
            _notification = notification;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

            var passwordFromDb = _profile.GetPassword(accountId);
            
            var hashPassword = _hash.GetHash(password);

            var currentOtp = _otp.GetCurrentOtp(accountId);

            if (string.Equals(currentOtp, otp) && string.Equals(passwordFromDb, hashPassword))
            {
                _failedCounter.Reset(accountId);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);
                _logger.Info($"accountId : {accountId}, failedTimes : {failedCount}");
                _notification.PushMessage($"accountId :{accountId} verify failed");

                return false;
            }
        }
    }
}