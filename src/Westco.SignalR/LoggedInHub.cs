using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Westco.SignalR
{
    [HubName("loggedInHub")]
    public class LoggedInHub : Hub
    {
        // Test method to be called by SignalR client       
        public void ClientBroadcastTime()
        {
            Clients.Caller.NotifyConnectionTime(System.DateTime.Now.ToLongTimeString());
        }
    }
}