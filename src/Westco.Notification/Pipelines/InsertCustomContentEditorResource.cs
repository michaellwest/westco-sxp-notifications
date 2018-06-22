using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.ContentEditor.Pipelines.RenderContentEditor;
using Page = Sitecore.Web.UI.HtmlControls.Page;

namespace Westco.Notification.Pipelines
{
    public class InsertCustomContentEditorResource
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

        public void Process(RenderContentEditorArgs args)
        {
            Assert.ArgumentNotNull(args, "args");

            if (!(HttpContext.Current.Handler is Page page)) return;

            foreach (var script in _scripts)
            {
                page.Form.Controls.Add(new LiteralControl($"<script type='text/javascript' src='{script}'></script>"));
            }

            foreach (var css in _styles)
            {
                page.Form.Controls.Add(new LiteralControl($"<link rel='stylesheet' type='text/css' href='{css}'/>"));
            }
        }
    }
}
