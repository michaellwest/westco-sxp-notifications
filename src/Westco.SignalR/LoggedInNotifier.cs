using Microsoft.AspNet.SignalR;
using Sitecore.Pipelines.LoggedIn;

namespace Westco.SignalR
{
    public class LoggedInNotifier
    {
        public void Process(LoggedInArgs args)
        {
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(
                new
                {
                    title = "User LoggedIn",
                    body = $"Username: {args.Username}",
                    icon = "http://sc82u5.local/temp/iconcache/Office/32x32/fire.png"
                }
                );

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LoggedInHub>();
            hubContext.Clients.All.ShowLoggedInUserInfo(jsonData);
        }
    }
}
