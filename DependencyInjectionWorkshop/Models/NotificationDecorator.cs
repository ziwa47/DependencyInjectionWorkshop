using DependencyInjectionWorkshop.Adapter;

namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator :IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        private void NotificationVerify(string accountId)
        {
            _notification.PushMessage($"accountId :{accountId} verify failed");
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid =  _authenticationService.Verify(accountId, password, otp);
            if (isValid == false)
            {
                NotificationVerify(accountId);
            }

            return isValid;
        }
    }
}