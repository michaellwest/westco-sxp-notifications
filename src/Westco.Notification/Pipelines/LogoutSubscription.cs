using Sitecore.Pipelines.Logout;

namespace Westco.Notification.Pipelines
{
    public class LogoutSubscription
    {
        public void Process(LogoutArgs args)
        {
            var data = new SubscriptionEventArgs()
            {
                Username = Sitecore.Context.User.Name
            };

            Sitecore.Events.Event.RaiseEvent("westcosocket:unsubscribe", data);
        }
    }
}
