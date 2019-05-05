using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Exceptions;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        private bool CheckAccountIsLocked(string accountId)
        {
            return _failedCounter.CheckAccountIsLocked(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            if (CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var isValid = base.Verify(accountId, password, otp);

            if (isValid)
            {
                _failedCounter.Reset(accountId);
            }
            else
            {
                _failedCounter.Add(accountId);
            }

            return isValid;
        }
    }
}