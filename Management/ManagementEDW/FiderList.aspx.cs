using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OlcuYonetimSistemi.Controllers.EDW;
using OlcuYonetimSistemi.Models.Edw;

namespace OlcuYonetimSistemi.Management.ManagementEDW
{
    public partial class FiderList : System.Web.UI.Page
    {

        private Fider.ListFilter m_ListFilter = null;
        private Fider.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new Fider.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as Fider.ListFilter);
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
                this.MasterPageFile = "~/Management/ManagementEDW/ModalDialog.Master";
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
                if (IsSelectMode)
                {
                    pnlAddEdit.Visible = false;
                }
                if (Request["TransformerCenterId"] != null && Information.IsNumeric(Request["TransformerCenterId"].ToString().Trim()))
                {
                    int id = Convert.ToInt32(Request["TransformerCenterId"].ToString().Trim());
                    var FiderC = TransformerCenterFider.GetTransformerCenterFider(id);
                    if (FiderC != null)
                    {
                        txtTransformerCenterF.Text = FiderC.Name;
                        hfTransformerCenterIdF.Value = FiderC.Id.ToString();
                        lbRemoveTransformerCenterF.Visible = false;
                        lbTransformerCenterF.Visible = false;
                        btnSearch_Click(btnSearch, e);
                        return;
                    }
                }
                ListFider(1, true);
            }

        }
        Object IdTypes;
 

        private void ListFider(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;

            Int32 totalRows = -1;
            if (calcPage)
            {
                grdPmum.DataSource = Fider.ListFider(startIndex, ucPager.PageSize, ListFilter, ref totalRows);

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
                grdPmum.DataSource = Fider.ListFider(startIndex, ucPager.PageSize, ListFilter);
                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grdPmum.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(hfTransformerCenterIdF.Value))
            {
                ListFilter.TransformerCenterId = Convert.ToInt32(hfTransformerCenterIdF.Value);
            }
            else ListFilter.TransformerCenterId = null;
            if (!String.IsNullOrWhiteSpace(txtNameF.Text))
            {
                ListFilter.Name = txtNameF.Text.Trim();
                if (ListFilter.Name.ToString().IndexOf('%') < 0) ListFilter.Name = '%' + ListFilter.Name + '%';
            }
            else ListFilter.Name = null;
            ListFider(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveIdNameChanges.Visible = false;
            btnSaveChanges.Visible = true;
            tId.Visible = true;
            dvName.Visible = true;
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;

            txtComment.Text = string.Empty;
            txtName.Text = String.Empty;
            txtId.Text = string.Empty;
            txtTransformerCenter.Text = string.Empty;
            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt";
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
}});", pnlAddEdit.ClientID), true);
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            return; //bu sayfada ekleme ve güncelleme işlemi yapoılmayacağından 

            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;
            var item = Fider.GetFider(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.Visible = true;
                tId.Visible = false;
                dvName.Visible = true;
                btnSaveIdNameChanges.Visible = false;
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.Id.ToString();
                txtName.Text = item.Name;
                txtComment.Text = item.Comment;
                txtId.Text = item.Id.ToString();
                txtTransformerCenter.Text = item.TransformerCenterName;
                hfTransformerCenterId.Value = item.TransformerCenterId.ToString();
                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
}});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            return; //bu sayfada ekleme ve güncelleme işlemi yapoılmayacağından 
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);
            if (string.IsNullOrEmpty(hfTransformerCenterId.Value))
            {
                ucAlert.Text = "Trafo Merkezi Seçiniz.";
                txtTransformerCenter.Focus();
                return;
            }
          
            if (String.IsNullOrWhiteSpace(txtName.Text))
            {
                ucAlert.Text = "Adı girmelisiniz.";
                txtName.Focus();
                return;
            }
            bool isEdit = (btnSave.CommandName != "A" ? false : true);

            EdwFider fider = new EdwFider
            {
                Id = !String.IsNullOrWhiteSpace(txtId.Text.Trim()) ? Convert.ToInt32(txtId.Text.Trim()) : -1,
                Name = txtName.Text.Trim(),
                TransformerCenterId = !String.IsNullOrWhiteSpace(hfTransformerCenterId.Value) ? Convert.ToInt32(hfTransformerCenterId.Value) : -1,
                Comment = txtComment.Text
            };
            OlcuYonetimSistemi.DbHelper.DbResponse<EdwFider> saveResp = Fider.SaveFider(fider);
            switch (saveResp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    upAddEdit.Update();

                    lbRefreshPage.CommandArgument = ucPager.CurrentPage.ToString();
                    lbRefreshPage.CommandName = (btnSave.CommandName == "A" ? "CALCPAGE" : String.Empty);
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

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            return; //bu sayfada ekleme ve güncelleme işlemi yapoılmayacağından 
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument)) return;
            string[] commandArgs = btnDelete.CommandArgument.ToString().Split(new char[] { ';' });
            int id = Convert.ToInt32(commandArgs[0]);
            var resp = Fider.DeleteFider(id);
            switch (resp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.NotFound:
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    ListFider((Int32)ViewState["StartRowIndex"], true);
                    break;
                default:
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, resp.StatusDescription, true), true);
                    break;
            }
        }

        protected void btnSelectItem_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            if (String.IsNullOrEmpty(SelectCallback)) return;
            Button btnSelectItem = (sender as Button);
            var item = Fider.GetFider(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
            if (item != null)
            {
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", String.Format("$(function() {{ window.parent.{0}({1}); }});", SelectCallback, ser.Serialize(item)), true);
            }
        }

        protected string GetDialogUrl(int pmumId, string command)
        {
            string title = "";
            string url = "";
            UriBuilder uri = null;
            switch ((command ?? String.Empty).ToUpper())
            {
                case "CHANGETransformerCenter":
                    uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/ManagementEDW/FiderList.aspx")));
                    uri.Query = "TransformerCenterId=" + pmumId.ToString();
                    return uri.ToString();
            }
            return String.Format("javascript:showTransformerCenter('{0}', '{1}')", HttpUtility.JavaScriptStringEncode(title, false), HttpUtility.JavaScriptStringEncode(url, false));
        }
        protected string ParseIdType(object typeId)
        {
            if (typeId == null)
                return string.Empty;
            return EnumHelper.ParseEnum<Models.InavitasIdType>(typeId).GetDisplayName();
        }
        protected string GetMeterPointUrl(int pmumTypeHistoryId)
        {
            UriBuilder uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/ManagementEDW/EquipmentList.aspx")));
            uri.Query = "pmumTypeHistoryId=" + pmumTypeHistoryId.ToString();
            return uri.ToString();
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
                ListFider(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListFider(newIndex, true);
        }



        protected void btnSaveIdNameChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);
            
            if (String.IsNullOrWhiteSpace(txtName.Text))
            {
                ucAlert.Text = "Trafo Adı girmelisiniz.";
                txtName.Focus();
                return;
            }
          
            bool isEdit = (btnSave.CommandName != "A" ? false : true);
            EdwFider fider = new EdwFider
            {
                Id = !String.IsNullOrWhiteSpace(txtId.Text.Trim()) ? Convert.ToInt32(txtId.Text.Trim()) : -1,
                Name = txtName.Text.Trim(),
                TransformerCenterId = !String.IsNullOrWhiteSpace(hfTransformerCenterId.Value) ? Convert.ToInt32(hfTransformerCenterId.Value) : -1,
            };
            OlcuYonetimSistemi.DbHelper.DbResponse<EdwFider> saveResp = Fider.SaveFider(fider);
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

        protected void lbSrcTransformerCenter_Click(object sender, EventArgs e)
        {
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showTransformerCenter('Trafo Merkezi Seçimi','/Management/ManagementEDW/FiderTransformerCenter.aspx?wmode=select&callback=SelectTransformerCenter');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }

        protected void lbRmvTransformerCenterF_Click(object sender, EventArgs e)
        {
            hfTransformerCenterIdF.Value = String.Empty;
            txtTransformerCenterF.Text = String.Empty;
        }

    }
}