using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter
    {
        public void PushMessage(string account)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(msg => { }, "my channel", $"{account}", "my bot name");
        }
    }
}