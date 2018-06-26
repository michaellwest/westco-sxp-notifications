using Sitecore.Pipelines.LoggingIn;

namespace Westco.Notification.Pipelines
{
    public class LoggingInSubscription
    {
        public void Process(LoggingInArgs args)
        {
            var data = new SubscriptionEventArgs()
            {
                Username = args.Username
            };

            Sitecore.Events.Event.RaiseEvent("westcosocket:subscribe", data);
        }
    }
}
