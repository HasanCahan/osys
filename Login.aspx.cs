using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Web.Configuration;
using System.Text;


namespace OlcuYonetimSistemi
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack && (User != null && User.Identity != null && User.Identity.IsAuthenticated))
            {
                FormsAuthentication.SignOut();
            }
        }

        protected void btngiris_Click(object sender, EventArgs e)
        {            
            bool authParam = false;
            MembershipUser user = null;

            lblDurum.Text = null;
            if (string.IsNullOrWhiteSpace(txtUserName.Text) || String.IsNullOrWhiteSpace(txtUserPass.Text)) goto endpoint;
            user = Membership.GetUser(username: txtUserName.Text.Trim());           
            
            if (user == null) goto endpoint;

            if (!user.IsApproved)
            {
                lblDurum.Text = "Kullanıcı hesabınız pasif durumda, giriş yapamazsınız.";
                goto endpoint;
            }

            if (user.IsLockedOut)
            {
                if (user.LastLockoutDate.AddMinutes(Membership.PasswordAttemptWindow) > DateTime.UtcNow)
                {
                    lblDurum.Text = "Çok sayıda hatalı giriş denemesi nedeniyle hesabınız kilitli, birkaç dakika sonra yeniden deneyiniz.";
                    goto endpoint;
                }
                else if (!user.UnlockUser())
                {
                    lblDurum.Text = "Hesabınızın kilidi kaldırılamıyor.";
                    goto endpoint;
                }
            }

            if (Controllers.CustomMembership.IsDomainUser(user.UserName))
            {
                /*
                if (servisKimlikList.SelectedValue == "EKSIM")
                {
                    using (eksimService.EKSIMServiceClient clientEksim = new eksimService.EKSIMServiceClient())
                    {
                        clientEksim.ClientCredentials.UserName.UserName = "ososportal";
                        clientEksim.ClientCredentials.UserName.Password = "Osp+8131";
                        eksimService.GenericReturnOfboolean valid = clientEksim.LDAPValidate(servisKimlikList.SelectedValue, txtUserName.Text.Trim(), txtUserPass.Text.Trim());
                        authParam = (valid.Success && valid.Data == true);
                        clientEksim.Close();
                    }
                }
                else*/
                if (servisKimlikList.SelectedValue == "DEDAS")
                {
                    using (WSDEDAS.DEDASServiceClient clientDedas = new WSDEDAS.DEDASServiceClient())
                    {
                        clientDedas.ClientCredentials.UserName.UserName = "ososportal";
                        clientDedas.ClientCredentials.UserName.Password = "Osp+8131";
                        WSDEDAS.GenericReturnOfboolean valid = clientDedas.LDAPValidate(servisKimlikList.SelectedValue, txtUserName.Text.Trim(), txtUserPass.Text.Trim());
                        authParam = (valid.Success && valid.Data == true);
                        clientDedas.Close();
                    }
                }

                Controllers.CustomMembership.UpdateLoginAttempt(user.UserName, authParam);
            }
            else
            {
                authParam = Membership.ValidateUser(txtUserName.Text.Trim(), txtUserPass.Text.Trim());
            }

        endpoint:
            if (authParam == true)
            {
                FormsAuthentication.SetAuthCookie(user.UserName, false);
                Response.Redirect(FormsAuthentication.DefaultUrl,false);
            }
            else
            {
                if (String.IsNullOrEmpty(lblDurum.Text)) lblDurum.Text = "Hatalı Kullanıcı veya Şifre";
                lblDurum.ForeColor = System.Drawing.Color.Red;
                txtUserName.Text = "";
                txtUserPass.Text = "";
                txtUserName.Focus();
            }
        }
    }
}
