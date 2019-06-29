namespace DependencyInjectionWorkshop.Models
{
    public class IsAccountLockDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly IFailedCounter _failedCounter;
        
        public IsAccountLockDecorator(IAuthentication authentication,IFailedCounter failedCounter)
        {
            _authentication = authentication;
            _failedCounter = failedCounter;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked) 
                throw new FailedTooManyTimesException();

            return _authentication.Verify(account, password, otp);
        }
    }
}