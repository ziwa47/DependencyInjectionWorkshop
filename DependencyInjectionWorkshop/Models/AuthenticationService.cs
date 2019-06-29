namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly NlogAdapter _nlogAdapter = new NlogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var currentPassword = _profileDao.GetPassword(account);
            var hashPassword = _sha256Adapter.Hash(password);
            var currentOtp = _otpService.GetOtpResp(account);

            if (hashPassword == currentPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(account);
                return true;
            }
            else
            {
                _slackAdapter.PushMessage(account);

                _failedCounter.AddFailedCount(account);

                int failedCount = _failedCounter.GetFailedCount(account);
                _nlogAdapter.Info($"account:{account} failed times:{failedCount}");
                
                return false;
            }
        }
    }
}