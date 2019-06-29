namespace DependencyInjectionWorkshop.Models
{
    public abstract class DecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected DecoratorBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string account, string password, string otp)
        {
            return _authentication.Verify(account, password, otp);
        }
    }

    public class NotificationDecorator : DecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
            : base(authentication)
        {
            _notification = notification;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isValid = base.Verify(account, password, otp);
            if (!isValid)
            {
                _notification.PushMessage(account);
            }

            return isValid;
        }
    }
}