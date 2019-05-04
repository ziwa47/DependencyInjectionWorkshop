using SlackAPI;

namespace DependencyInjectionWorkshop.Adapter
{
    public class SlackAdapter
    {
        public void Notify(string errMsg)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", errMsg, "my bot name");
        }
    }
}