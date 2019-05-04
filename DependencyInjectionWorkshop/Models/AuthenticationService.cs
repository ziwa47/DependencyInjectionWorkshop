using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Repository;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOpt _opt;
        private readonly ILogger _Logger;
        private readonly INotification _notification;

        public AuthenticationService(IFailedCounter failedCounter, IProfile profile, IHash hash,
            IOpt opt, ILogger logger, INotification notification)
        {
            _failedCounter = failedCounter;
            _profile = profile;
            _hash = hash;
            _opt = opt;
            _Logger = logger;
            _notification = notification;
        }

        public AuthenticationService()
        {
            _failedCounter = new FailedCounter();
            _profile = new ProfileRepo();
            _hash = new Sha256Adapter();
            _opt = new OptService();
            _Logger = new NLogAdapter();
            _notification = new SlackAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            _failedCounter.CheckAccountIsLocked(accountId);

            var passwordFromDb = _profile.GetPassword(accountId);

            var hashPassword = _hash.GeHash(password);

            var currentOtp = _opt.GetCurrentOtp(accountId);

            if (string.Equals(currentOtp, otp) && string.Equals(passwordFromDb, hashPassword))
            {
                _failedCounter.Reset(accountId);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                var failedCount = _failedCounter.Get(accountId);
                _Logger.Info($"accountId : {accountId}, failedTimes : {failedCount}");
                _notification.PushMessage($"accountId :{accountId} verify failed");

                return false;
            }
        }
    }
}