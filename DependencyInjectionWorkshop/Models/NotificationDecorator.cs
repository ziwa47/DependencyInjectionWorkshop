using DependencyInjectionWorkshop.Adapter;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator :AuthenticationBaseDecorator
    {
        private readonly INotification _notification;
        private readonly ILogger _logger;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification, ILogger logger) : base(authenticationService)
        {
            _notification = notification;
            _logger = logger;
        }

        private void NotificationVerify(string accountId)
        {
            _logger.Info("GG");
            _notification.PushMessage($"accountId :{accountId} verify failed");
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid =  base.Verify(accountId, password, otp);
            if (isValid == false)
            {
                NotificationVerify(accountId);
            }

            return isValid;
        }
    }
}