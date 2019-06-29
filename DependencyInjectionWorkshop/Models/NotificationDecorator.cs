namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthenticationService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthenticationService authenticationService, INotification notification)
        {
            _authenticationService = authenticationService;
            _notification = notification;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = _authenticationService.Verify(account, password, otp);
            if (!isValid)
            {
                _notification.PushMessage(account);
            }

            return isValid;
        }
    }
}