using DependencyInjectionWorkshop.Adapter;

namespace DependencyInjectionWorkshop.Models
{
    public class LogDecorator :IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogDecorator(IAuthentication authenticationService, IFailedCounter failedCounter, ILogger logger)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        private void LogVerify(string accountId)
        {
            var failedCount = _failedCounter.Get(accountId);
            _logger.Info($"accountId : {accountId}, failedTimes : {failedCount}");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authenticationService.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogVerify(accountId);
            }

            return isValid;
        }
    }
}