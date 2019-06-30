namespace DependencyInjectionWorkshop.Models
{
    public abstract class BaseDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected BaseDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string account, string password, string otp)
        {
            return _authentication.Verify(account, password, otp);
        }
    }
}