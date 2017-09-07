using Microsoft.AspNet.SignalR;
using Sitecore.Pipelines.LoggedIn;
using Sitecore.Resources;
using Sitecore.Web.UI;

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
                    icon = Images.GetThemedImageSource("Office/32x32/fire.png", ImageDimension.id32x32)
                }
                );

            var hubContext = GlobalHost.ConnectionManager.GetHubContext<LoggedInHub>();
            hubContext.Clients.All.ShowLoggedInUserInfo(jsonData);
        }
    }
}
