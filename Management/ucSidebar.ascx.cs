using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class ucSidebar : System.Web.UI.UserControl
    {

        [Description("Aktif menü"), Category("Appearance")]
        public string ActiveMenuRel
        {
            get
            {
                return (ViewState["ActiveMenuRel"] ?? String.Empty).ToString();
            }
            set
            {
                ViewState["ActiveMenuRel"] = (value ?? String.Empty).Trim();
            }
        }

        [Description("Navigasyon çubuğu"), Category("Appearance")]
        public bool IsNavbar
        {
            get
            {
                return Convert.ToBoolean(ViewState["IsNavbar"] ?? false);
            }
            set
            {
                ViewState["IsNavbar"] = value;
            }
        }
        void setEysMenuVisibility(bool visibility)
        {
            //mzFormulationList.Visible = visibility;
            mzStatusHistoryList.Visible = visibility;
            mzTransformerCenterList.Visible = visibility;
            mzTransformerList.Visible = visibility;
            //mzFiderList.Visible = visibility;
            mzFiderIdDegis.Visible = visibility;
        }
        void setEysMenuAdminOptionVisibility(bool visibility)
        {
            mzStatusList.Visible = visibility;
        }
        protected void Page_Load(object sender, EventArgs e)
        {


            
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW") && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE"))
            {
                mEquipmentList.Visible = false;
                mReadoutList.Visible = false;
                mMeteredAreaList.Visible = false;
                mMeterPointList.Visible = false;
                mTown.Visible = false;
                mzFiderIdDegis.Visible = true;
            }
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE") && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW"))
            {
                setEysMenuVisibility(false);
                mzStatusList.Visible = false;
                mzCity.Visible = false;
                mzFiderIdDegis.Visible = false;
            }
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ADMIN") && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE") && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW"))
            {
                mEquipmentList.Visible = false;
                mReadoutList.Visible = false;
                mMeteredAreaList.Visible = false;
                mMeterPointList.Visible = false;
                mTown.Visible = false;
                mzCity.Visible = false;
                setEysMenuVisibility(false);
                mzAdminPanel.Visible = true;
                mUserList.Visible = true; 
                mzFiderIdDegis.Visible = true;
            }
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ADMIN"))
            {
                mzAdminPanel.Visible = true;
                mUserList.Visible = true;
                setEysMenuAdminOptionVisibility(true);
            }
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ACMA") &&!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ADMIN") && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "OYSILCE") && !Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "EDW"))
            {
                mEquipmentList.Visible = false;
                mReadoutList.Visible = false;
                mMeteredAreaList.Visible = false;
                mMeterPointList.Visible = false;
                mTown.Visible = false;
                mzCity.Visible = false;
                setEysMenuVisibility(false);
                mzAdminPanel.Visible = true;
                mUserList.Visible = false;
                mzFiderIdDegis.Visible = false;
                Li1.Visible=true;
            }
            if(!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ACMA"))
            {
                Li1.Visible=false;

            }
            setEysMenuAdminOptionVisibility(false);
            //#region temp   //Todo should be commented when ready to use
            //setEysMenuAdminOptionVisibility(false);
            //setEysMenuVisibility(false);
            //#endregion
        }

        public override void RenderControl(HtmlTextWriter writer)
        {
            if (!IsNavbar)
            {
                List<HtmlGenericControl> items = sidebarList.Controls.Cast<object>().Where(x => x is HtmlGenericControl).Cast<HtmlGenericControl>().ToList();
                HtmlGenericControl active = items.Where(x => (x.Attributes["class"] ?? String.Empty) == "active").FirstOrDefault();
                HtmlGenericControl current = null;
                if (!String.IsNullOrEmpty(this.ActiveMenuRel) && (active == null || (active.Attributes["data-rel"] ?? String.Empty).Trim().Equals(this.ActiveMenuRel, StringComparison.InvariantCultureIgnoreCase) == false))
                {
                    current = items.Where(x => (x.Attributes["data-rel"] ?? String.Empty).Trim().Equals(this.ActiveMenuRel, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                    if (current != null) current.Attributes["class"] = "active";
                    if (active != null) active.Attributes.Remove("class");
                }
                if (active == null && current == null) items[0].Attributes.Add("class", "active");
            }
            else
            {
                sidebarList.Attributes["class"] = "nav navbar-nav pull-right";
                sidebarList.Attributes.Remove("id");
            }

            base.RenderControl(writer);
        }

        protected void lbLogout_Click(object sender, EventArgs e)
        {
            Session.Abandon();
            FormsAuthentication.RedirectToLoginPage();
        }
    }
}