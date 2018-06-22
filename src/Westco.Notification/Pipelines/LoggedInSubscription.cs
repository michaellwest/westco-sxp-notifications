using Sitecore.Pipelines.LoggedIn;

namespace Westco.Notification.Pipelines
{
    public class LoggedInSubscription
    {
        public void Process(LoggedInArgs args)
        {
            var data = new SubscriptionEventArgs()
            {
                Username = args.Username,
                SessionId = args.Context.Session.SessionID
            };

            Sitecore.Events.Event.RaiseEvent("westcosocket:subscribe", data);
        }
    }
}
