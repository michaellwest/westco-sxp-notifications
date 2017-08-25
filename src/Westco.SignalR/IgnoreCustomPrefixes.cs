using System;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.HttpRequest;

namespace Westco.SignalR
{
    public class IgnoreCustomPrefixes : HttpRequestProcessor
    {
        public override void Process(HttpRequestArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            string[] prefixes = { "/signalr/" };

            if (prefixes.Length <= 0) return;

            var filePath = args.Url.FilePath;
            foreach (var prefix in prefixes)
            {
                if (!filePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

                args.AbortPipeline();
                return;
            }
        }

    }
}