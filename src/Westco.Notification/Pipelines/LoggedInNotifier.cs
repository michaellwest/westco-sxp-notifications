using System;
using Sitecore.Pipelines.LoggedIn;
using Sitecore.Resources;
using Sitecore.Web.UI;

namespace Westco.Notification.Pipelines
{
    public class LoggedInNotifier
    {
        public void Process(LoggedInArgs args)
        {
            var data = new MessageEventArgs
            {
                SessionId = args.Context.Session.SessionID,
                CanBroadcast = true,
                Title = "User LoggedIn",
                Body = $"Username: {args.Username}",
                Icon = Images.GetThemedImageSource("Office/32x32/fire.png", ImageDimension.id32x32)
            };

            Sitecore.Events.Event.RaiseEvent("westcosocket:notify", data);
        }
    }
}
