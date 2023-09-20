using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OlcuYonetimSistemi.Controllers.EDW;
using OlcuYonetimSistemi.Models.Edw;
using System.Web.Security;

namespace OlcuYonetimSistemi.Management.ManagementEDW
{
    public partial class StatusList : System.Web.UI.Page
    {

        protected override void OnPreInit(EventArgs e)
        {
            if (Request["wmode"] != null && Request["wmode"].Trim().ToUpper(Helper.enCulture) == "SELECT")
            {
                this.MasterPageFile = "~/Management/ManagementEDW/ModalDialog.Master";
            }
            base.OnPreInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ADMIN"))
            {
                Response.Redirect("~/Default.aspx");
                return;
            }
            if (!Page.IsPostBack)
            {
                ListStatus();
            }
        }

        private void ListStatus()
        {
            grStatus.DataSource = Status.ListStatus();
            grStatus.DataBind();
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;
            txtName.Text = String.Empty;
            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt";
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;
            var item = Status.GetStatus(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.Id.ToString();
                txtId.Text = item.Id.ToString();
                txtName.Text = item.Name;
                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);
            if (String.IsNullOrWhiteSpace(txtName.Text))
            {
                ucAlert.Text = "Statü Adı girmelisiniz.";
                txtName.Focus();
                return;
            }

            EdwStatus statu = new EdwStatus()
            {
                Id = btnSave.CommandName == "A" ? 0 : Convert.ToInt32(btnSave.CommandArgument),
                Name = txtName.Text
            };

            OlcuYonetimSistemi.DbHelper.DbResponse<EdwStatus> saveResp = Status.SaveStatus(statu);
            switch (saveResp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    upAddEdit.Update();

                    ScriptManager.RegisterClientScriptBlock(upAddEdit, upAddEdit.GetType(), "jsAddEdit"
                        , String.Format(@"$('#{0}').modal('hide'); {1};", pnlAddEdit.ClientID
                        , Page.ClientScript.GetPostBackEventReference(lbRefreshPage, String.Empty))
                        , true);
                    break;
                default:
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                    break;
            }
        }
        protected void lblRefresh_Click(object sender, CommandEventArgs e)
        {

            ListStatus();

        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument) || !Information.IsNumeric(btnDelete.CommandArgument)) return;
            OlcuYonetimSistemi.DbHelper.DbResponse resp = Status.DeleteStatus(Convert.ToInt32(btnDelete.CommandArgument.Trim()));
            switch (resp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.NotFound:
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    ListStatus();
                    break;
                default:
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, resp.StatusDescription, true), true);
                    break;
            }
        }
        protected string GetMeterPointUrl(int statusId)
        {
            UriBuilder uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/ManagementEDW/StatusList.aspx")));
            uri.Query = "statusId=" + statusId.ToString();
            return uri.ToString();
        }

    }
}