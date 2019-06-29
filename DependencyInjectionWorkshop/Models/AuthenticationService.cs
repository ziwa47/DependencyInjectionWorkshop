namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthenticationService
    {
        bool Verify(string account, string password, string otp);
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;
        private readonly IProfileDao _profileDao;
        private readonly IHash _hash;

        public AuthenticationService(IFailedCounter failedCounter, ILogger logger, IOtpService otpService, IProfileDao profileDao, IHash hash)
        {
            _failedCounter = failedCounter;
            _logger = logger;
            _otpService = otpService;
            _profileDao = profileDao;
            _hash = hash;
        }

        public bool Verify(string account, string password, string otp)
        {
            var currentPassword = _profileDao.GetPassword(account);
            var hashPassword = _hash.Hash(password);
            var currentOtp = _otpService.GetOtpResp(account);

            if (hashPassword == currentPassword && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(account);
                return true;
            }

            _failedCounter.AddFailedCount(account);

            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.Info($"account:{account} failed times:{failedCount}");
            return false;
        }
    }
}