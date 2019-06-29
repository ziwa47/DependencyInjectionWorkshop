namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly FailedCounter _failedCounter;
        private readonly SlackAdapter _slackAdapter;
        private readonly NlogAdapter _nlogAdapter;

        public AuthenticationService(ProfileDao profileDao, Sha256Adapter sha256Adapter, OtpService otpService, FailedCounter failedCounter, SlackAdapter slackAdapter, NlogAdapter nlogAdapter)
        {
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _slackAdapter = slackAdapter;
            _nlogAdapter = nlogAdapter;
        }
        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _slackAdapter = new SlackAdapter();
            _nlogAdapter = new NlogAdapter();
        }

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