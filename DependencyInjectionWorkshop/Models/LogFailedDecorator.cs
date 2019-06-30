namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedDecorator : BaseDecorator
    {
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogFailedDecorator(IAuthentication authentication, ILogger logger, IFailedCounter failedCounter) : base(authentication)
        {
            _logger = logger;
            _failedCounter = failedCounter;
        }

        private void LogFailedCount(string account)
        {
            var failedCount = _failedCounter.GetFailedCount(account);
            _logger.Info($"account:{account} failed times:{failedCount}");
        }

        public override bool Verify(string account, string password, string otp)
        {
            var verify = base.Verify(account, password, otp);
            if (!verify)
            {
                LogFailedCount(account);
            }
            return verify;
        }
    }
}