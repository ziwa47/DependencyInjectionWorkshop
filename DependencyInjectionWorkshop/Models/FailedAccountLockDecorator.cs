namespace DependencyInjectionWorkshop.Models
{
    public class FailedAccountLockDecorator : BaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedAccountLockDecorator(IAuthentication authentication, IFailedCounter failedCounter) 
            : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked)
                throw new FailedTooManyTimesException();

            var verify = base.Verify(account, password, otp);

            if (!verify)
            {
                _failedCounter.AddFailedCount(account);
            }
            else
            {
                _failedCounter.ResetFailedCount(account);
            }

            return verify;
        }
    }
}