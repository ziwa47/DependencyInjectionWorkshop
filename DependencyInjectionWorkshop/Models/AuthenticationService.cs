namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;
        private readonly IProfileDao _profileDao;
        private readonly IHash _hash;
        private readonly INotification _notification;
        
        public AuthenticationService(IProfileDao profileDao, IHash hash, IOtpService otpService,
            IFailedCounter failedCounter, INotification notification, ILogger logger)
        {
            _profileDao = profileDao;
            _hash = hash;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _notification = notification;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _hash = new Sha256Adapter();
            _profileDao = new ProfileDao();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _notification = new SlackAdapter();
            _logger = new NlogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked) throw new FailedTooManyTimesException();

            var currentPassword = _profileDao.GetPassword(account);
            var hashPassword = _hash.Hash(password);
            var currentOtp = _otpService.GetOtpResp(account);

            if (hashPassword == currentPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(account);
                return true;
            }

            _notification.PushMessage(account);

            _failedCounter.AddFailedCount(account);

            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.Info($"account:{account} failed times:{failedCount}");

            return false;
        }
    }
}