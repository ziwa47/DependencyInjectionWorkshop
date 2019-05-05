using System;
using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Repository;

namespace DependencyInjectionWorkshop.Models
{
    public class ApiCheckTimeDecorator :AuthenticationBaseDecorator
    {
        private readonly IApiUserQuotaV2 _apiUserQuotaV2;

        public ApiCheckTimeDecorator(IAuthentication authentication, IApiUserQuotaV2 apiUserQuotaV2) : base(authentication)
        {
            _apiUserQuotaV2 = apiUserQuotaV2;
        }

        private bool CheckUseTimes(string accountId)
        {
            return _apiUserQuotaV2.CheckUseTimes(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            if (CheckUseTimes(accountId))
                throw new CheckUseTimesException();
            var isValid = base.Verify(accountId, password, otp);
            return isValid;
        }
    }

    public class CheckUseTimesException : Exception
    {
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtp _otp;
        private readonly IProfile _profile;

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

            var isValid = currentOtp == otp
                          && passwordFromDb == hashPassword;

            return isValid;
        }
    }

    public interface IApiUserQuotaV2
    {
        bool CheckUseTimes(string accountId);
    }

    public class ApiUserQuotaV2 : IApiUserQuotaV2
    {
        public bool CheckUseTimes(string accountId)
        {
            throw new System.NotImplementedException();
        }
    }
}