namespace DependencyInjectionWorkshop.Models
{
    public class IsAccountLockDecorator : IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IFailedCounter _failedCounter;

        public IsAccountLockDecorator(IAuthenticationService authenticationService,IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked) 
                throw new FailedTooManyTimesException();

            return _authenticationService.Verify(account, password, otp);
        }
    }
}