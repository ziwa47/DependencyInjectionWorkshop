namespace DependencyInjectionWorkshop.Models
{
    public class ApiUserDecorator : AuthenticationBaseDecorator
    {
        private readonly IApiUserQuota _apiUserQuota;

        public ApiUserDecorator(IAuthentication authentication, IApiUserQuota apiUserQuota) : base(authentication)
        {
            _apiUserQuota = apiUserQuota;
        }

        private void AddApiUseTimes(string accountId)
        {
            _apiUserQuota.Add(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            AddApiUseTimes(accountId);
            return isValid;
        }
    }
}