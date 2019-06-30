using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models
{
    public class MethodLogDecorator : BaseDecorator
    {
        private readonly ILogger _logger;

        public MethodLogDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var currentMethod = MethodBase.GetCurrentMethod();
            var name = currentMethod.Name;

            var msg = $"{nameof(Authentication)}.{name}, account :{account},otp :{otp}";
            _logger.Info(msg);
            var isValid = base.Verify(account, password, otp);
            _logger.Info(msg + $", result : {isValid}");
            return isValid;
        }
    }
}
