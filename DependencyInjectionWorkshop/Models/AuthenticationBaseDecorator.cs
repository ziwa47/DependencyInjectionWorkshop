namespace DependencyInjectionWorkshop.Models
{
    public abstract class AuthenticationBaseDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected AuthenticationBaseDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string accountId, string password, string otp)
        {
            return _authentication.Verify(accountId, password, otp);
        }
    }
}