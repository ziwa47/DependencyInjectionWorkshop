using DependencyInjectionWorkshop.Adapter;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator :AuthenticationBaseDecorator
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification) : base(authenticationService)
        {
            _notification = notification;
        }

        private void NotificationVerify(string accountId)
        {
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