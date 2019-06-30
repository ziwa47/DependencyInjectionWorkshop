using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            //IProfile profile = new FakeProfile();
            ////IProfile profile = new ProfileRepo();
            //IHash hash = new FakeHash();
            ////IHash hash = new Sha256Adapter();
            //IOtpService otpService = new FakeOtp();
            ////IOtp otpService = new OtpService();
            //IFailedCounter failedCounter = new FakeFailedCounter();
            ////IFailedCounter failedCounter = new FailedCounter();
            //ILogger logger = new ConsoleAdapter();
            ////ILogger logger = new NLogAdapter();
            //INotification notification = new FakeSlack();
            ////INotification notification = new SlackAdapter();


            //var authenticationService =
            //    new Authentication(otpService, profile, hash);

            //var notificationDecorator = new NotificationDecorator(authenticationService, notification);
            //var failedCounterDecorator = new FailedAccountLockDecorator(notificationDecorator, failedCounter);
            //var logDecorator = new LogFailedDecorator(failedCounterDecorator, logger, failedCounter);

            //var finalAuthentication = logDecorator;

            //var isValid = finalAuthentication.Verify("joey", "pw", "123457");

            RegisterContainer();
            var authentication = _container.Resolve<IAuthentication>();
            var isValid = authentication.Verify("joey", "pw", "123457");

            Console.WriteLine(isValid);
            Console.ReadLine();
        }

        private static void RegisterContainer()
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterType<FakeHash>().As<IHash>();
            containerBuilder.RegisterType<FakeOtp>().As<IOtpService>();
            containerBuilder.RegisterType<FakeProfile>().As<IProfile>();
            containerBuilder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            containerBuilder.RegisterType<ConsoleAdapter>().As<ILogger>();
            containerBuilder.RegisterType<SlackAdapter>().As<INotification>();

            containerBuilder.RegisterType<Authentication>().As<IAuthentication>();
            containerBuilder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            containerBuilder.RegisterDecorator<FailedAccountLockDecorator, IAuthentication>();
            containerBuilder.RegisterDecorator<LogFailedDecorator, IAuthentication>();
            _container = containerBuilder.Build();
        }
    }

    internal class ConsoleAdapter : ILogger

    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
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
        public void ResetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(ResetFailedCount)}({accountId})");
        }

        public void AddFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetOtpResp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetOtpResp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string Hash(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Hash)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}
