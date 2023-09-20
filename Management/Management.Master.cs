using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class Management : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ucSidebar.ActiveMenuRel = Page.GetType().Name;
                if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW") && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE"))
                {
                    ltProjectName.Text = "Enerji Yönetim Sistemi";
                }
                else if (!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW") && Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE"))
                {
                    ltProjectName.Text = "Ölçü Yönetim Sistemi";
                }
                else
                {
                    ltProjectName.Text = "Enerji - Ölçü Yönetim Sistemi";                    
                }
            }
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Response.Redirect(ResolveUrl("~") + "Login.aspx");
        }
    }
}