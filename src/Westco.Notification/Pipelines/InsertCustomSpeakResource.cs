using System.Collections.Generic;
using Sitecore.Diagnostics;
using Sitecore.Web.Pipelines.GetPageScripts;

namespace Westco.Notification.Pipelines
{
    public class InsertCustomSpeakResource : GetPageScriptsProcessor
    {
        private readonly IList<string> _scripts = new List<string>();

        public void AddScriptResource(string resource)
        {
            _scripts.Add(resource);
        }

        public override void Process(GetPageScriptsArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            foreach (var script in _scripts)
            {
                args.FileNames.Add(new ScriptFile { Name = script });
            }
        }
    }

    public class ScriptFile
    {
        public string Name { get; set; }

        public bool IsModule { get; set; }
    }
}
