namespace DependencyInjectionWorkshop.Models
{
    public class Authentication : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IProfileDao _profileDao;

        public Authentication(IOtpService otpService, IProfileDao profileDao, IHash hash)
        {
            _otpService = otpService;
            _profileDao = profileDao;
            _hash = hash;
        }

        public bool Verify(string account, string password, string otp)
        {
            var currentPassword = _profileDao.GetPassword(account);
            var hashPassword = _hash.Hash(password);
            var currentOtp = _otpService.GetOtpResp(account);

            return hashPassword == currentPassword && otp == currentOtp;
        }
    }
}