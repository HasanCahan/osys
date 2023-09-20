using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class ucAlert : System.Web.UI.UserControl
    {
        public enum AlertTypes : byte
        {
            Default = 0,
            Success = 1,
            Info = 2,
            Warning = 3,
            Danger = 4
        }

        [Description("Uyarı biçimi"), Category("Appearance")]
        public AlertTypes AlertType { get; set; }
        private List<string> m_Lines = new List<string>();
        [Description("Uyarı mesaj satırları"), Category("Appearance")]
        public List<string> Lines { get { return m_Lines; } }
        [Description("Uyarı mesajı"), Category("Appearance")]
        public string Text { get { return String.Join(Environment.NewLine, m_Lines); } set { m_Lines.Clear(); m_Lines.Add(value); } }

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            this.Visible = !String.IsNullOrWhiteSpace(Text);
            string css = "alert ";
            switch (this.AlertType)
            {
                case AlertTypes.Danger:
                    css += "alert-danger";
                    break;
                case AlertTypes.Success:
                    css += "alert-success";
                    break;
                case AlertTypes.Warning:
                    css += "alert-warning";
                    break;
                case AlertTypes.Info:
                case AlertTypes.Default:
                default:
                    css += "alert-info";
                    break;
            }

            Panel alert = new Panel();
            alert.CssClass = css;
            alert.Attributes.Add("role", "alert");
            alert.Controls.Add(new LiteralControl(this.Text));
            this.Controls.Add(alert);

            base.RenderControl(writer);
        }
    }
}