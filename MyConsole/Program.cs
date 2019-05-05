using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DependencyInjectionWorkshop.Adapter;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Repository;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            //var _profile = new FakeProfile();
            //var _otp =new FakeOtp();
            //var _hash = new FakeHash();
            //var _notification = new FakeSlack();
            //var _logger = new ConsoleAdapter();
            //var _failedCounter = new FakeFailedCounter();

            //var authentication =
            //    new AuthenticationService(_profile, _hash, _otp);

            //var notificationDecorator = new NotificationDecorator(authentication,_notification);
            //var failedCounterDecorator = new FailedCounterDecorator(notificationDecorator,_failedCounter);
            //var logDecorator = new LogDecorator(failedCounterDecorator, _failedCounter, _logger);
            //var _authentication = logDecorator;
            //_authentication.Verify("ziwa", "123456", "123456");

            RegisterContainer();

            IAuthentication authentication = _container.Resolve<IAuthentication>();
            authentication.Verify("joey", "my hashed password", "123456");
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();


            builder.RegisterType<FakeOtp>().As<IOtp>();
            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeHash>().As<IHash>();

            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<ConsoleAdapter>().As<ILogger>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();

            _container = builder.Build();
        }


        internal class FakeSlack : INotification
        {
            public void PushMessage(string message)
            {
                Console.WriteLine($"{nameof(FakeSlack)}.{nameof(PushMessage)}({message})");
            }
        }

        internal class FakeFailedCounter : IFailedCounter
        {
            public void Reset(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
            }

            public void Add(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
            }

            public int Get(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({accountId})");
                return 91;
            }

            public bool CheckAccountIsLocked(string accountId)
            {
                Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(CheckAccountIsLocked)}({accountId})");
                return false;
            }
        }

        internal class FakeOtp : IOtp
        {
            public string GetCurrentOtp(string accountId)
            {
                Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
                return "123456";
            }
        }

        internal class FakeHash : IHash
        {
            public string GetHash(string plainText)
            {
                Console.WriteLine($"{nameof(FakeHash)}.{nameof(GetHash)}({plainText})");
                return "my hashed password";
            }
        }

        internal class FakeProfile : IProfile
        {
            public string GetPassword(string accountId)
            {
                Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
                return "my hashed passwordd";
            }
        }
    }

}
