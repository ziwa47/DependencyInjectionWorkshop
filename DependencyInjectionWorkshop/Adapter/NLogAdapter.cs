using System;

namespace DependencyInjectionWorkshop.Adapter
{
    public interface ILogger
    {
        void Info(string message);
    }

    public class NLogAdapter : ILogger
    {
        public void Info(string message)
        {
            NLog.LogManager.GetCurrentClassLogger().Info(message);
        }
    }

    public class ConsoleAdapter : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}