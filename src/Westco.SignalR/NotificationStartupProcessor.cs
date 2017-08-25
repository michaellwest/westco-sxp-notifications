using Microsoft.Owin;
using Owin;
using Sitecore.Pipelines;
using Westco.SignalR;

[assembly: OwinStartup(typeof(NotificationStartupProcessor))]

namespace Westco.SignalR
{
    public class NotificationStartupProcessor
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }

        public virtual void Process(PipelineArgs args)
        {
            
        }
    }
}