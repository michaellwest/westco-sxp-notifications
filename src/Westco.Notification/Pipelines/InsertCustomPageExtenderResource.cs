using System.Collections.Generic;
using System.IO;
using Sitecore.Diagnostics;
using Sitecore.Mvc.ExperienceEditor.Pipelines.RenderPageExtenders;

namespace Westco.Notification.Pipelines
{
    public class InsertCustomPageExtenderResource
    {
        private readonly IList<string> _scripts = new List<string>();
        private readonly IList<string> _styles = new List<string>();

        public void AddStyleResource(string resource)
        {
            _styles.Add(resource);
        }

        public void AddScriptResource(string resource)
        {
            _scripts.Add(resource);
        }

        public void Process(RenderPageExtendersArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            Render(args.Writer);
        }

        protected void Render(TextWriter output)
        {
            foreach (var script in _scripts)
            {
                output.Write($"<script type='text/javascript' src='{script}'></script>");
            }

            foreach (var css in _styles)
            {
                output.Write($"<link rel='stylesheet' type='text/css' href='{css}'/>");
            }
            /*
            foreach (string style in _styles)
            {
                output.Write(CssLinkPattern, style);
            }

            foreach (string script in _scripts)
            {
                output.Write(Sitecore.Web.HtmlUtil.GetClientScriptIncludeHtml(script));
            }*/
        }
    }
}
