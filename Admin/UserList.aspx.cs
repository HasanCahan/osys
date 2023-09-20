using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using OlcuYonetimSistemi.Management.ManagementEDW;

namespace OlcuYonetimSistemi.Admin
{
    public partial class UserList : System.Web.UI.Page
    {
        private Controllers.EDW.User.ListFilter m_ListFilter = null;
        private Controllers.EDW.User.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new Controllers.EDW.User.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as Controllers.EDW.User.ListFilter);
                    }
                }
                return m_ListFilter;
            }
        }
        protected bool IsSelectMode
        {
            get
            {
                return (bool)(ViewState["IsSelectMode"] ?? false);
            }
            set
            {
                ViewState["IsSelectMode"] = value;
            }
        }
        protected string SelectCallback
        {
            get
            {
                return (string)(ViewState["SelectCallback"] ?? String.Empty);
            }
            set
            {
                ViewState["SelectCallback"] = value;
            }
        }
        protected override void OnPreInit(EventArgs e)
        {
            if (Request["wmode"] != null && Request["wmode"].Trim().ToUpper(Helper.enCulture) == "SELECT")
            {
                this.MasterPageFile = "~/Management/ModalDialog.Master";
                this.IsSelectMode = true;
                if (this.IsSelectMode && !String.IsNullOrWhiteSpace(Request["callback"]))
                {
                    this.SelectCallback = Request["callback"].Trim();
                }
            }
            base.OnPreInit(e);
        }

        private const string dateFormat = "dd.MM.yyyy";   //"dd.MM.yyyy hh:mm"

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ddlCity.DataSource = Controllers.CityTown.ListCity();
                ddlCity.DataBind();
                ddlCity.Items.Insert(0, new ListItem("- il seçiniz -", String.Empty));
            }
            if (Roles.IsUserInRole(HttpContext.Current.User.Identity.Name, "ADMIN"))
            {
                if (!Page.IsPostBack)
                {
                    if (IsSelectMode)
                    {
                        pnlAddEdit.Visible = false;
                    }
                    else
                    {

                    }
                    ListUser(1, true);
                }
            }
            else
            {
                Response.Redirect("/Default.aspx");
            }
        }

        protected void PageNumber_Click(object sender, CommandEventArgs e)
        {
            if (!String.IsNullOrEmpty((e.CommandArgument ?? String.Empty).ToString()))
            {
                bool calcPage = ((e.CommandName ?? String.Empty) == "CALCPAGE" ? true : false);

                int newIndex = 0;
                int startIndex = (int)ViewState["StartRowIndex"];
                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                switch ((e.CommandArgument ?? String.Empty).ToString().ToUpper())
                {
                    case "NEXT":
                        newIndex = pagingIndex.Where(x => x.Value > startIndex).OrderBy(x => x.Value).FirstOrDefault().Value;
                        break;
                    case "PREV":
                        newIndex = pagingIndex.Where(x => x.Value < startIndex).OrderByDescending(x => x.Value).FirstOrDefault().Value;
                        break;
                    default:
                        newIndex = pagingIndex.Where(x => x.Key == Convert.ToInt32(e.CommandArgument)).FirstOrDefault().Value;
                        break;
                }
                ListUser(newIndex, true);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListUser(newIndex, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Info;
            ucAlert.Text = "Şifre domain şifresi olarak güncellenecektir.";

            dvFormGroup.Visible = false;
            lbltxtPassword.Visible = false;
            txtPassword.Visible = false;
            lbltxtEMail.Visible = true;
            txtEMail.Visible = true;
            lbltxtUserName.Visible = true;
            txtUserName.Visible = true;
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;
            txtUserId.Text = String.Empty;
            txtUserName.Text = String.Empty;
            txtPassword.Text = String.Empty;
            txtEMail.Text = String.Empty;
            btnSaveChanges.Visible = true;
            btnSaveRoleChanges.Visible = false;
            btnRemoveRoleChanges.Visible = false;
            txtUserName.ReadOnly = false;
            txtPassword.ReadOnly = false;
            txtEMail.ReadOnly = false;
            ddlCity.Visible = true;
            lblddlCity.Visible = true;

            ddlCity.SelectedValue = "";

            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt";
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            dvFormGroup.Visible = false;
            btnSaveChanges.Visible = true;
            btnRemoveRoleChanges.Visible = false;
            btnSaveRoleChanges.Visible = false;
            txtUserName.ReadOnly = true;
            txtPassword.ReadOnly = true;
            lbltxtPassword.Visible = true;
            txtPassword.Visible = true;
            lbltxtEMail.Visible = true;
            txtEMail.Visible = true;
            lbltxtUserName.Visible = true;
            txtUserName.Visible = true;
            txtEMail.ReadOnly = false;
            ddlCity.Visible = true;
            lblddlCity.Visible = true;
            ddlCity.Enabled = true;

            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument)) return;
            Models.User item = Controllers.EDW.User.GetUser(new Guid(btnChange.CommandArgument.Trim()));

            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.UserId.ToString();

                txtUserId.Text = item.UserId.ToString();
                txtUserName.Text = item.UserName;
                txtPassword.Text = item.Password;
                txtEMail.Text = item.EMail;
                ddlCity.SelectedValue = "";
                if (item.IlId!=null && item.IlId>0)
                ddlCity.SelectedValue =item.IlId.ToString();

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument)) return;

            DbHelper.DbResponse resp = Controllers.EDW.User.DeleteUser(btnDelete.CommandArgument.Trim());
            switch (resp.StatusCode)
            {
                case DbHelper.DbResponseStatus.NotFound:
                case DbHelper.DbResponseStatus.OK:
                    ListUser((Int32)ViewState["StartRowIndex"], true);
                    break;
                default:
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, resp.StatusDescription, true), true);
                    break;
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);

            if (String.IsNullOrWhiteSpace(txtUserName.Text))
            {
                ucAlert.Text = "Kullanıcı Adı girmelisiniz.";
                txtUserName.Focus();
                return;
            }

            if (String.IsNullOrWhiteSpace(txtPassword.Text))
            {
                // Şifre domain şifresi ile güncelleneceğinden membershipin kabul edeceği formatta şifre oluşturuldu.
                txtPassword.Text = "Aa12345!";
            }

            if (String.IsNullOrWhiteSpace(txtEMail.Text))
            {
                ucAlert.Text = "Kullanıcı E-Mail girmelisiniz.";
                txtEMail.Focus();
                return;
            }

            //if (String.IsNullOrWhiteSpace(ddlCity.SelectedValue))
            //{
            //    ucAlert.Text = "İl bilgisi seçmediniz";
            //    txtEMail.Focus();
            //    return;
            //}


            bool isEdit = (btnSave.CommandName == "A" ? false : true);
            Int32.TryParse(ddlCity.SelectedValue, out int ilId);
            Models.User user = new Models.User()
            {
                UserId = new Guid(),
                UserName = txtUserName.Text.Trim(),
                EMail = txtEMail.Text.Trim(),
                CreateDate = DateTime.Now,
                Status = (isEdit == true) ? 1 : 0,
                Password = txtPassword.Text.Trim(),
                IlId = ilId
            };

            //using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required,TimeSpan.FromSeconds(30)))
            //{
                DbHelper.DbResponse<Models.User> saveResp = Controllers.EDW.User.SaveUser(user);
                if (saveResp.StatusCode == DbHelper.DbResponseStatus.Conflict)
                {
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                }
                else if (saveResp.StatusCode == DbHelper.DbResponseStatus.OK)
                {
                    bool cancelled = false;
                    if (!cancelled)
                    {
                        Controllers.EDW.User.UpdateDomainUsers(user.UserName);
                        //scope.Complete();
                        lbRefreshPage.CommandArgument = ucPager.CurrentPage.ToString();
                        lbRefreshPage.CommandName = (btnSave.CommandName == "E" || btnSave.CommandName == "A" ? "CALCPAGE" : String.Empty);
                        upAddEdit.Update();
                        ScriptManager.RegisterClientScriptBlock(upAddEdit, upAddEdit.GetType(), "jsAddEdit"
                            , String.Format(@"$('#{0}').modal('hide'); {1};", pnlAddEdit.ClientID
                            , Page.ClientScript.GetPostBackEventReference(lbRefreshPage, String.Empty))
                            , true);
                    }
                }
                else
                {
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                }
            //}
        }

        private void ListUser(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;

            Int32 totalRows = -1;
            if (calcPage)
            {
                if (!String.IsNullOrWhiteSpace(txtUserNameF.Text.Trim()))
                {
                    ListFilter.UserName = txtUserNameF.Text.Trim();
                    if (ListFilter.UserName.IndexOf('%') < 0) ListFilter.UserName = '%' + ListFilter.UserName + '%';
                }
                else ListFilter.UserName = null;

                if (!String.IsNullOrWhiteSpace(txtUserEmailF.Text.Trim()))
                {
                    ListFilter.EMail = txtUserEmailF.Text.Trim();
                    if (ListFilter.EMail.IndexOf('%') < 0) ListFilter.EMail = '%' + ListFilter.EMail + '%';
                }
                else ListFilter.EMail = null;

                if (!String.IsNullOrWhiteSpace(txtReadBeginDateF.Text))
                {
                    DateTime tmp;
                    if (DateTime.TryParseExact(txtReadBeginDateF.Text.Trim(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tmp))
                    {
                        ListFilter.BeginDate = tmp;
                    }
                    else ListFilter.BeginDate = null;
                }
                else ListFilter.BeginDate = null;

                if (!String.IsNullOrWhiteSpace(ddlRoleTypeF.SelectedValue) && ddlRoleTypeF.SelectedValue != "0")
                {
                    ListFilter.RoleName = '%' + ddlRoleTypeF.SelectedValue.Trim() + '%';
                }
                else ListFilter.RoleName = null;

                if (!String.IsNullOrWhiteSpace(ddlRoleTypeF.SelectedValue) && ddlRoleTypeF.SelectedValue != "0")
                {
                    ListFilter.IlId = '%' + ddlCity.SelectedValue.Trim() + '%';
                }
                else ListFilter.IlId = null;

                grdUser.DataSource = Controllers.EDW.User.ListUser(startIndex, ucPager.PageSize, ListFilter, ref totalRows);
                Dictionary<int, int> pagingIndex = new Dictionary<int, int>();
                int pnum = 0;
                for (int i = 1; i <= totalRows; i += ucPager.PageSize)
                {
                    pnum++;
                    pagingIndex.Add(pnum, i);
                }
                ViewState["PagingIndex"] = pagingIndex;
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.TotalPage = pagingIndex.Count;
                ucPager.TotalRecord = totalRows;
                ucPager.DataBind();
            }
            else
            {
                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grdUser.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtUserNameF.Text.Trim()))
            {
                ListFilter.UserName = txtUserNameF.Text.Trim();
                if (ListFilter.UserName.IndexOf('%') < 0) ListFilter.UserName = '%' + ListFilter.UserName + '%';
            }
            else ListFilter.UserName = null;

            if (!String.IsNullOrWhiteSpace(txtUserEmailF.Text.Trim()))
            {
                ListFilter.EMail = txtUserEmailF.Text.Trim();
                if (ListFilter.EMail.IndexOf('%') < 0) ListFilter.EMail = '%' + ListFilter.EMail + '%';
            }

            if (!String.IsNullOrWhiteSpace(txtReadBeginDateF.Text))
            {
                DateTime tmp;
                if (DateTime.TryParseExact(txtReadBeginDateF.Text.Trim(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tmp))
                {
                    ListFilter.BeginDate = tmp;
                }
                else ListFilter.BeginDate = null;
            }
            else ListFilter.BeginDate = null;

            if (!String.IsNullOrWhiteSpace(ddlRoleTypeF.SelectedValue) && ddlRoleTypeF.SelectedValue != "0")
            {
                ListFilter.RoleName = '%' + ddlRoleTypeF.SelectedValue.Trim() + '%';
            }
            else ListFilter.RoleName = null;

            ListUser(1, true);
        }

        protected void btnSelectItem_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            if (String.IsNullOrEmpty(SelectCallback)) return;
            Button btnSelectItem = (sender as Button);
            Models.User item = Controllers.EDW.User.GetUser(new Guid(btnSelectItem.CommandArgument.Trim()));
            if (item != null)
            {
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", String.Format("$(function() {{ window.parent.{0}({1}); }});", SelectCallback, ser.Serialize(item)), true);
            }
        }

        protected void btnAddRole_Click(object sender, EventArgs e)
        {
            dvFormGroup.Visible = true;
            btnSaveRoleChanges.CommandName = "E";
            btnSaveRoleChanges.CommandArgument = String.Empty;
            txtUserId.Text = String.Empty;
            txtUserName.Text = String.Empty;
            txtPassword.Text = String.Empty;
            txtEMail.Text = String.Empty;
            txtEMail.ReadOnly = true;
            lbltxtPassword.Visible = false;
            txtPassword.Visible = false;
            lbltxtEMail.Visible = false;
            txtEMail.Visible = false;
            lbltxtUserName.Visible = false;
            txtUserName.Visible = false;
            btnRemoveRoleChanges.Visible = false;
            btnSaveChanges.Visible = false;
            btnSaveRoleChanges.Visible = true;
            cbCityRole.Checked = false;
            cbCityRole.Enabled = true;
            cbTownRole.Checked = false;
            cbTownRole.Enabled = true;
            cbAdminRole.Checked = false;
            cbAdminRole.Enabled = true;
            cbAcmaRole.Checked = false;
            cbAcmaRole.Enabled = true;
            ddlCity.Visible = false;
            lblddlCity.Visible = false;
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument)) return;
            Models.User item = Controllers.EDW.User.GetUser(new Guid(btnChange.CommandArgument.Trim()));
            ddlCity.SetSelectedVal(item.IlId);
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.UserId.ToString();
                List<string> roleNameList = item.RoleName.Split(',').ToList();

                foreach (var roleItem in roleNameList)
                {
                    switch (roleItem)
                    {
                        case "ADMIN":
                            cbAdminRole.Checked = true;
                            cbAdminRole.Enabled = false;
                            break;
                        case "EDW":
                            cbCityRole.Checked = true;
                            cbCityRole.Enabled = false;
                            break;
                        case "OYSILCE":
                            cbTownRole.Checked = true;
                            cbTownRole.Enabled = false;
                            break;
                        case "ACMA":
                            cbAcmaRole.Checked = true;
                            cbAcmaRole.Enabled = false;
                            break;
                        default:
                            break;
                    };
                }
                txtUserId.Text = item.UserId.ToString();
                txtUserName.Text = item.UserName;
                txtPassword.Text = item.Password;
                txtEMail.Text = item.EMail;

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kullanıcı Rol Tanımı : " + item.UserName;
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnSaveRoleChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);

            List<string> RoleList = new List<string>();

            if (cbAdminRole.Checked && cbAdminRole.Enabled == true)
            {
                RoleList.Add("ADMIN");
            }
            if (cbCityRole.Checked && cbCityRole.Enabled == true)
            {
                RoleList.Add("EDW");
            }
            if (cbTownRole.Checked && cbTownRole.Enabled == true)
            {
                RoleList.Add("OYSILCE");
            }
            if (cbAcmaRole.Checked && cbAcmaRole.Enabled == true)
            {
                RoleList.Add("ACMA");
            }
            if (RoleList.Count == 0)
            {
                ucAlert.Text = "Kullanıcı Rolü seçmelisiniz.";
                cbAdminRole.Focus();
                return;
            }

            bool isEdit = (btnSave.CommandName == "A" ? false : true);

            Models.User user = new Models.User()
            {
                UserId = new Guid(),
                UserName = txtUserName.Text.Trim(),
                RoleName = String.Join(",", RoleList.ToArray()),
                Status = (isEdit == true) ? 1 : 0
            };

            using (TransactionScope scope = new TransactionScope())
            {
                DbHelper.DbResponse<Models.User> saveResp = Controllers.EDW.User.SaveUserRole(user);
                if (saveResp.StatusCode == DbHelper.DbResponseStatus.Conflict)
                {
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                }
                else if (saveResp.StatusCode == DbHelper.DbResponseStatus.OK)
                {
                    bool cancelled = false;
                    user = saveResp.Data;

                    if (!cancelled)
                    {
                        scope.Complete();
                        lbRefreshPage.CommandArgument = ucPager.CurrentPage.ToString();
                        lbRefreshPage.CommandName = (btnSave.CommandName == "E" || btnSave.CommandName == "A" ? "CALCPAGE" : String.Empty);
                        upAddEdit.Update();
                        ScriptManager.RegisterClientScriptBlock(upAddEdit, upAddEdit.GetType(), "jsAddEdit"
                            , String.Format(@"$('#{0}').modal('hide'); {1};", pnlAddEdit.ClientID
                            , Page.ClientScript.GetPostBackEventReference(lbRefreshPage, String.Empty))
                            , true);
                    }
                }
                else
                {
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                    ucAlert.Text = "Kayıt yapılamıyor, lütfen yeniden deneyiniz." + saveResp.StatusDescription;
                }
            }
        }

        protected void btnRemoveRole_Click(object sender, EventArgs e)
        {
            dvFormGroup.Visible = true;
            btnRemoveRoleChanges.CommandName = "E";
            btnRemoveRoleChanges.CommandArgument = String.Empty;
            btnRemoveRoleChanges.Visible = true;
            txtUserId.Text = String.Empty;
            txtUserName.Text = String.Empty;
            txtPassword.Text = String.Empty;
            txtEMail.Text = String.Empty;
            txtEMail.ReadOnly = true;
            lbltxtPassword.Visible = false;
            txtPassword.Visible = false;
            lbltxtEMail.Visible = false;
            txtEMail.Visible = false;
            lbltxtUserName.Visible = false;
            txtUserName.Visible = false;
            btnSaveChanges.Visible = false;
            btnSaveRoleChanges.Visible = false;
            cbCityRole.Enabled = false;
            cbCityRole.Checked = false;
            cbTownRole.Enabled = false;
            cbTownRole.Checked = false;
            cbAdminRole.Enabled = false;
            cbAdminRole.Checked = false;
            cbAcmaRole.Enabled = false;
            cbAcmaRole.Checked = false;
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument)) return;
            Models.User item = Controllers.EDW.User.GetUser(new Guid(btnChange.CommandArgument.Trim()));

            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.UserId.ToString();
                List<string> roleNameList = item.RoleName.Split(',').ToList();

                foreach (var roleItem in roleNameList)
                {
                    switch (roleItem)
                    {
                        case "ADMIN":
                            cbAdminRole.Enabled = true;
                            break;
                        case "EDW":
                            cbCityRole.Enabled = true;
                            break;
                        case "OYSILCE":
                            cbTownRole.Enabled = true;
                            break;
                        case "ACMA":
                            cbAcmaRole.Enabled = true;
                            break;
                        default:
                            break;
                    };
                }

                txtUserId.Text = item.UserId.ToString();
                txtUserName.Text = item.UserName;
                txtPassword.Text = item.Password;
                txtEMail.Text = item.EMail;

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Silinecek rolü seçmelisiniz. ";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);

            }
        }

        protected void btnRemoveRoleChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);

            List<string> RoleList = new List<string>();

            if (cbAdminRole.Checked && cbAdminRole.Enabled == true)
            {
                RoleList.Add("ADMIN");
            }
            if (cbCityRole.Checked && cbCityRole.Enabled == true)
            {
                RoleList.Add("EDW");
            }
            if (cbTownRole.Checked && cbTownRole.Enabled == true)
            {
                RoleList.Add("OYSILCE");
            }
            if (cbAcmaRole.Checked && cbAcmaRole.Enabled == true)
            {
                RoleList.Add("ACMA");
            }
            if (RoleList.Count == 0)
            {
                ucAlert.Text = "Silinecek rolü seçmelisiniz.";
                cbAdminRole.Focus();
                return;
            }

            bool isEdit = (btnSave.CommandName == "A" ? false : true);

            Models.User user = new Models.User()
            {
                UserId = new Guid(),
                UserName = txtUserName.Text.Trim(),
                RoleName = String.Join(",", RoleList.ToArray()),
                Status = (isEdit == true) ? 1 : 0
            };

            using (TransactionScope scope = new TransactionScope())
            {
                DbHelper.DbResponse<Models.User> saveResp = Controllers.EDW.User.RemoveUserRole(user);
                if (saveResp.StatusCode == DbHelper.DbResponseStatus.Conflict)
                {
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                }
                else if (saveResp.StatusCode == DbHelper.DbResponseStatus.OK)
                {
                    bool cancelled = false;
                    user = saveResp.Data;

                    if (!cancelled)
                    {
                        scope.Complete();
                        lbRefreshPage.CommandArgument = ucPager.CurrentPage.ToString();
                        lbRefreshPage.CommandName = (btnSave.CommandName == "E" || btnSave.CommandName == "A" ? "CALCPAGE" : String.Empty);
                        upAddEdit.Update();
                        ScriptManager.RegisterClientScriptBlock(upAddEdit, upAddEdit.GetType(), "jsAddEdit"
                            , String.Format(@"$('#{0}').modal('hide'); {1};", pnlAddEdit.ClientID
                            , Page.ClientScript.GetPostBackEventReference(lbRefreshPage, String.Empty))
                            , true);
                    }
                }
                else
                {
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                    ucAlert.Text = "Kayıt yapılamıyor, lütfen yeniden deneyiniz.";
                }
            }
        }

    }
}