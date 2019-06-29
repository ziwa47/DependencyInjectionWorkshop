namespace DependencyInjectionWorkshop.Models
{
    public class NlogAdapter
    {
        public void Info(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}