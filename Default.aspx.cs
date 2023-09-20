using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace OlcuYonetimSistemi
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW") && Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE"))
                {
                    Response.Redirect("/Management/ManagementEDW/Default.aspx",false);
                }
                else if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW"))
                {
                    Response.Redirect("/Management/ManagementEDW/Default.aspx",false);
                }
                else if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE"))
                {
                    Response.Redirect("Management/Default.aspx",false);
                }
                else if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ACMA"))
                {
                    Response.Redirect("/Management/ManagementEDW/FiderAcmaList.aspx",false);
                }
            }
        }
    }
}