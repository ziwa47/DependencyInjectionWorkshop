using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Exceptions;

namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator :IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        private bool CheckAccountIsLocked(string accountId)
        {
            return _failedCounter.CheckAccountIsLocked(accountId);
        }

        public bool Verify(string accountId, string password, string otp)
        {
            if (CheckAccountIsLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }

            var isValid= _authenticationService.Verify(accountId, password, otp);
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