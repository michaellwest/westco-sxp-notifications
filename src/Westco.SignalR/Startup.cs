using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using Sitecore.Pipelines;
using Westco.SignalR;

[assembly: OwinStartup(typeof(Startup))]

namespace Westco.SignalR
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);

            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(60);

            // This value must be no more than 1/3 of the DisconnectTimeout value.
            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(20);

            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = true };
            app.MapSignalR(hubConfiguration);
        }
    }
}