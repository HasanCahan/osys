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
using System.Data;
namespace OlcuYonetimSistemi.Management.ManagementEDW
{
    public partial class StatusHistoryList : System.Web.UI.Page
    {

        private StatusHistory.ListFilter m_ListFilter = null;
        private StatusHistory.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new StatusHistory.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as StatusHistory.ListFilter);
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
                ListStatusHistory(1, true);
                checkVisibilityOsos(ddlStatus);
                initializeControls(e);
                if (IsSelectMode)
                {
                    pnlAddEdit.Visible = false;
                }
                if (Request["TransformerCenterId"] != null && Information.IsNumeric(Request["TransformerCenterId"].ToString().Trim()))
                {
                    int id = Convert.ToInt32(Request["TransformerCenterId"].ToString().Trim());
                    var tc = TransformerCenter.GetTransformerCenter(id);
                    if (tc != null)
                    {
                        txtTransformerCenterF.Text = tc.Name;
                        hfTransformerCenterIdF.Value = tc.Id.ToString();
                        btnSearch_Click(btnSearch, e);
                        return;
                    }
                }
                if (Request["action"] != null)
                {
                    if (Request["action"] == "create")
                    {
                        btnNew_Click(sender, e);
                        return;
                    }
                }
            }
        }
        void initializeControls(EventArgs e)
        {

            fillDdlStatus(true);
            fillDdlStatus(false);
            fillDdlConsumption(true);
            fillDdlConsumption(false);
        }


        Object consumptionTypes;
        void fillDdlConsumption(Boolean searchControl)
        {
            if (consumptionTypes == null)
                consumptionTypes = EnumHelper.Enumerate<ConsumptionType>();
            List<DropDownList> ddowns = new List<DropDownList>();
            string text = searchControl ? "- Tümü -" : "- Seçiniz -";
            ddowns.Add(searchControl ? ddlEdwConsumptionF : ddlEdwConsumption);
            ddowns.Add(searchControl ? ddlBaraConsumptionF : ddlBaraConsumption);
            ddowns.Add(searchControl ? ddlFiderConsumptionF : ddlFiderConsumption);

            foreach (var ddown in ddowns)
            {
                ddown.DataSource = consumptionTypes;
                ddown.DataTextField = "Text";
                ddown.DataValueField = "Value";
                ddown.DataBind();
                ddown.Items.Insert(0, new ListItem(text, String.Empty));
            }
        }
        System.Data.DataSet statusData;

        void fillDdlStatus(Boolean searchControl)
        {
            if (statusData == null)
                statusData = Status.ListStatus();
            string text = searchControl ? "- Tümü -" : "- Seçiniz -";
            DropDownList ddown = searchControl ? ddlStatusF : ddlStatus;
            ddown.DataSource = statusData;
            ddown.DataTextField = "Name";
            ddown.DataValueField = "Id";
            ddown.DataBind();
            ddown.Items.Insert(0, new ListItem(text, String.Empty));
        }
        protected string ParseEnergyDirectionType(object typeId)
        {
            if (typeId == null)
                return string.Empty;
            return EnumHelper.ParseEnum<EnergyDirectionType>(typeId).GetDisplayName();
        }
        protected string ParseConsumptionType(object typeId)
        {
            if (typeId == null)
                return string.Empty;
            return EnumHelper.ParseEnum<ConsumptionType>(typeId).GetDisplayName();
        }
        protected string ParseMeasuerementType(object typeId)
        {
            if (typeId == null)
                return string.Empty;
            return EnumHelper.ParseEnum<MeasurementType>(typeId).GetDisplayName();
        }
        protected string GetImage(object endDate)
        {
            const string activeImage = @"~/Content/Images/active.png";
            const string passiveImage = @"~/Content/Images/passive.png";
            const string imagePattern = "<img src=\"{0}\" alt=\"{1}\" height=\"32\" width=\"32\">";
            if (endDate == null)
                return string.Format(imagePattern, ResolveUrl(activeImage), "Aktif");
            var dt = endDate as Nullable<DateTime>;
            if (dt == null)
                return string.Format(imagePattern, ResolveUrl(activeImage), "Aktif");
            else
                return string.Format(imagePattern, ResolveUrl(passiveImage), "Pasif");
        }
        private void ListStatusHistory(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;
            Int32 totalRows = -1;
            if (calcPage)
            {
                grdStatusHistory.DataSource = StatusHistory.ListStatusHistory(startIndex, ucPager.PageSize, ListFilter, ref totalRows);
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
                grdStatusHistory.DataSource = StatusHistory.ListStatusHistory(startIndex, ucPager.PageSize, ListFilter);
                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grdStatusHistory.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(hfTransformerCenterIdF.Value))
            {
                ListFilter.TransformerCenterId = Convert.ToInt32(hfTransformerCenterIdF.Value);
            }
            else ListFilter.TransformerCenterId = null;

            if (!String.IsNullOrWhiteSpace(hfTransformerIdF.Value))
            {
                ListFilter.TransformerId = Convert.ToInt32(hfTransformerIdF.Value);
            }
            else ListFilter.TransformerId = null;
            if (!string.IsNullOrEmpty(ddlEdwConsumptionF.SelectedValue))
                ListFilter.EdwConsumptionTypeId = Convert.ToInt32(ddlEdwConsumptionF.SelectedValue.Trim());
            else
                ListFilter.EdwConsumptionTypeId = null;
            if (!string.IsNullOrEmpty(ddlBaraConsumptionF.SelectedValue))
                ListFilter.BaraConsumptionTypeId = Convert.ToInt32(ddlBaraConsumptionF.SelectedValue.Trim());
            else
                ListFilter.BaraConsumptionTypeId = null;
            if (!string.IsNullOrEmpty(ddlFiderConsumptionF.SelectedValue))
                ListFilter.FiderConsumptionTypeId = Convert.ToInt32(ddlFiderConsumptionF.SelectedValue.Trim());
            else
                ListFilter.FiderConsumptionTypeId = null;
            if (!string.IsNullOrEmpty(ddlStatusF.SelectedValue))
                ListFilter.StatusId = Convert.ToInt32(ddlStatusF.SelectedValue);
            else
                ListFilter.StatusId = null;
            if (chckActiveF.Checked)
                ListFilter.HasEnded = false;
            else
                ListFilter.HasEnded = null;
            ListStatusHistory(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveChanges.Visible = true;
            tId.Visible = true;
            changeVisibility(true);
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;
            ddlStatus.ClearSelection();
            ddlEdwConsumption.ClearSelection();
            ddlBaraConsumption.ClearSelection();
            ddlFiderConsumption.ClearSelection();
            ddlEdwConsumption.SelectedValue = ConsumptionType.Edw.ToIntString();
            ddlBaraConsumption.SelectedValue = ConsumptionType.Tki.ToIntString();
            ddlFiderConsumption.SelectedValue = ConsumptionType.Fider.ToIntString();
            txtId.Text = string.Empty;
            txtComment.Text = string.Empty;
            txtTransformer.Text = string.Empty;
            hfTransformer.Value = string.Empty;
            txtCreationDate.Text = DateTime.Now.ToString();
            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt(Statü)";
            checkVisibilityOsos(ddlStatus);
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
}});", pnlAddEdit.ClientID), true);
        }
        void changeVisibility(Boolean visiblity)
        {
            dvPrimary.Visible = visiblity;
            tId.Visible = visiblity;
        }
        //TODO change 
        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            Boolean isChangeMeasurementCommand = btnChange.CommandName == "CHANGEMEASUREMENT";
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;
            var item = StatusHistory.GetStatusHistory(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.Visible = true;
                tId.Visible = false;
                btnSaveChanges.CommandName = !isChangeMeasurementCommand ? "E" : "C";///Comandtype C is changemeasurement;
                btnSaveChanges.CommandArgument = item.Id.ToString();
                ddlStatus.ClearSelection();
                ddlEdwConsumption.ClearSelection();
                ddlBaraConsumption.ClearSelection();
                ddlFiderConsumption.ClearSelection();
                txtId.Text = item.Id.ToString();
                txtOsosNumber.Text = item.OsosNumber.ToString();
                ddlStatus.SetSelectedVal(item.StatusId);
                ddlEdwConsumption.SetSelectedVal(item.EdwConsumptionTypeId);
                ddlBaraConsumption.SetSelectedVal(item.BaraConsumptionTypeId);
                ddlFiderConsumption.SetSelectedVal(item.FiderConsumptionTypeId);

                dvOsos.Visible = false;
                if (item.StatusId == 5)
                {
                    dvOsos.Visible = true;
                }
                
                txtTransformer.Text = item.TransformerName;
                txtCreationDate.Text = item.CreationTime.ToString("dd.MM.yyyy HH:mm");
                hfTransformer.Value = item.TransformerId.ToString();
                txtComment.Text = isChangeMeasurementCommand ? "" : item.Comment;
                upAddEdit.Visible = true;
                if (isChangeMeasurementCommand)
                    changeVisibility(false);
                ltAddEditName.Text = isChangeMeasurementCommand ? "Ölçüm Türü Değiştir" : "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
}});", pnlAddEdit.ClientID), true);
            }
        }
        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);
            if (String.IsNullOrWhiteSpace(ddlStatus.SelectedValue))
            {
                ucAlert.Text = "Statü Seçiniz.";
                ddlStatus.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtTransformer.Text))
            {
                ucAlert.Text = "Trafo Seçilmedi";
                txtTransformer.Focus();
                return;
            }
            if (string.IsNullOrEmpty(ddlEdwConsumption.SelectedValue) && string.IsNullOrEmpty(ddlBaraConsumption.SelectedValue) && string.IsNullOrEmpty(ddlFiderConsumption.SelectedValue))
            {
                ucAlert.Text = "Tüketim Tipleri, EDW-Bara-Fider için en az biri Girilmelidir";
                ddlEdwConsumption.Focus();
                return;
            }
            var statuTypeId = Convert.ToInt32(ddlStatus.SelectedValue);
            if (statuTypeId == 5 && string.IsNullOrEmpty(txtOsosNumber.Text) || Information.IsFindCharInNumeric(txtOsosNumber.Text.Trim()))
            {
                ucAlert.Text = "Osos Tesisat Numarası Gömülü Santral için Zorunludur ";
                txtOsosNumber.Focus();
                return;
            }
            else if (statuTypeId != 5)
            {
                txtOsosNumber.Text = "";
            }
            if (string.IsNullOrEmpty(txtCreationDate.Text))
                txtCreationDate.Text = DateTime.Now.ToString();
            bool isEdit = (btnSave.CommandName != "A");
            EdwStatusHistory statusH = null;
            if (isEdit)
                statusH = StatusHistory.GetStatusHistory(Convert.ToInt32(btnSave.CommandArgument));
            else
                statusH = new EdwStatusHistory();
            Boolean isChangeMeasurement = btnSave.CommandName == "C";
            if (isChangeMeasurement)
            {
                statusH.Id = 0;
                statusH.CreationTime = DateTime.Now;
            }
            if (!isChangeMeasurement)
            {
                statusH.StatusId = Convert.ToInt32(ddlStatus.SelectedValue);
                statusH.TransformerId = Convert.ToInt32(hfTransformer.Value);
            }
            DateTime dt;
            string[] pattern = { "dd/MM/yyyy hh:mm:ss", "dd.MM.yyyy hh:mm:ss", "dd/MM/yyyy", "dd.MM.yyyy", "dd/MM/yyyy hh:mm", "dd.MM.yyyy hh:mm" };
            bool hasDate = DateTime.TryParseExact(txtCreationDate.Text, pattern, null, System.Globalization.DateTimeStyles.None, out dt);
            if (hasDate)
                statusH.CreationTime = dt;
            if (statusH.CreationTime.Date > DateTime.Now.Date)
            {
                txtCreationDate.Focus();
                ucAlert.Text = "İşlem Tarihi bugünden büyük olamaz";
                return;
            }
            if (!string.IsNullOrEmpty(ddlEdwConsumption.SelectedValue))
                statusH.EdwConsumptionTypeId = Convert.ToInt32(ddlEdwConsumption.SelectedValue);
            if (!string.IsNullOrEmpty(ddlBaraConsumption.SelectedValue))
                statusH.BaraConsumptionTypeId = Convert.ToInt32(ddlBaraConsumption.SelectedValue);
            if (!string.IsNullOrEmpty(ddlFiderConsumption.SelectedValue))
                statusH.FiderConsumptionTypeId = Convert.ToInt32(ddlFiderConsumption.SelectedValue);
            statusH.Comment = txtComment.Text;
            if (!String.IsNullOrEmpty(txtOsosNumber.Text))
                statusH.OsosNumber = Convert.ToInt32(txtOsosNumber.Text);
            else
                statusH.OsosNumber = null;
            OlcuYonetimSistemi.DbHelper.DbResponse<EdwStatusHistory> saveResp = StatusHistory.SaveStatusHistory(statusH);
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
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument)) return;
            string[] commandArgs = btnDelete.CommandArgument.ToString().Split(new char[] { ';' });
            int id = Convert.ToInt32(commandArgs[0]);
            var resp = StatusHistory.DeleteStatusHistory(id);
            switch (resp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.NotFound:
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    ListStatusHistory((Int32)ViewState["StartRowIndex"], true);
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
            var item = StatusHistory.GetStatusHistory(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
            if (item != null)
            {
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", String.Format("$(function() {{ window.parent.{0}({1}); }});", SelectCallback, ser.Serialize(item)), true);
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
                ListStatusHistory(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListStatusHistory(newIndex, true);
        }


        protected void lbRemoveTransformerF_Click(object sender, EventArgs e)
        {
            hfTransformerIdF.Value = String.Empty;
            txtTransformerF.Text = String.Empty;
        }

        protected void lbTransformerF_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfTransformerCenterIdF.Value))
            {
                ucAlert.Text = "Trafo Seçimi için Öncelikle Trafo Merkezi seçilmelidir.";
                txtTransformerCenterF.Focus();
                return;
            }
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Trafo Seçimi','/Management/ManagementEDW/TransformerList.aspx?wmode=select&callback=SelectTransformer&TransformerCenterId=" + hfTransformerCenterIdF.Value + "');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }
        protected void lbSrcTransformerCenter_Click(object sender, EventArgs e)
        {
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Trafo Merkezi Seçimi','/Management/ManagementEDW/TransformerCenterList.aspx?wmode=select&callback=SelectTransformerCenter');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }

        protected void lbRmvTransformerCenterCenterF_Click(object sender, EventArgs e)
        {
            hfTransformerCenterIdF.Value = String.Empty;
            txtTransformerCenterF.Text = String.Empty;
        }

        protected void lbSrcTransformer_Click(object sender, EventArgs e)
        {
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Trafo Seçimi','/Management/ManagementEDW/TransformerList.aspx?wmode=select&callback=SelectTransformer');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }
        protected void checkVisibilityOsos(DropDownList control)
        {

            if (control == null)
                return;
            if (string.IsNullOrEmpty(control.SelectedValue))
            {
                dvOsos.Visible = false;
                return;
            }
            var id = Convert.ToInt32(control.SelectedValue);
            if (id == 5)
            {
                if (!dvOsos.Visible)
                    dvOsos.Visible = true;
            }
            else
                dvOsos.Visible = false;
        }
        protected void ddlStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            checkVisibilityOsos(sender as DropDownList);
        }


    }
}
