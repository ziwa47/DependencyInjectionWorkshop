namespace DependencyInjectionWorkshop.Models
{
    public class Authentication : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;

        public Authentication(IOtpService otpService, IProfile profile, IHash hash)
        {
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
        }

        public bool Verify(string account, string password, string otp)
        {
            var currentPassword = _profile.GetPassword(account);
            var hashPassword = _hash.Hash(password);
            var currentOtp = _otpService.GetOtpResp(account);

            return hashPassword == currentPassword && otp == currentOtp;
        }
    }
}