using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void PushMessage(string account);
    }

    public class SlackAdapter : INotification
    {
        public void PushMessage(string account)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(msg => { }, "my channel", $"{account}", "my bot name");
        }
    }
}