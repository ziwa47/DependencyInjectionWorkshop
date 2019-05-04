namespace DependencyInjectionWorkshop.Adapter
{
    public class NLogAdapter
    {
        public void LogFailedCount(string message)
        {
            NLog.LogManager.GetCurrentClassLogger().Info(message);
        }
    }
}