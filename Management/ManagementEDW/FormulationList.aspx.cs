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
    public partial class FormulationList : System.Web.UI.Page
    {

        private Formulation.ListFilter m_ListFilter = null;
        private Formulation.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new Formulation.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as Formulation.ListFilter);
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (IsSelectMode)
                {
                    pnlAddEdit.Visible = false;
                }
                ListFormulation(1, true);
                fillDdlStatus(true);
                fillDdlStatus(false);
                fillDdlMeasurementType(true);
                fillDdlMeasurementType(false);
                filDddlEnergyDirection(true);
                filDddlEnergyDirection(false);
            }
        }
        Object measurementTypes;
        void fillDdlMeasurementType(Boolean searchControl)
        {
            if (measurementTypes == null)
                measurementTypes = EnumHelper.Enumerate<MeasurementType>();
            string text = searchControl ? "- Tümü -" : "- Seçiniz -";
            DropDownList ddown = searchControl ? ddlMeasurementTypeF : ddlMeasurementType;
            ddown.DataSource = measurementTypes;
            ddown.DataTextField = "Text";
            ddown.DataValueField = "Value";
            ddown.DataBind();
            ddown.Items.Insert(0, new ListItem(text, String.Empty));
        }


        Object energyDirection;
        void filDddlEnergyDirection(Boolean searchControl)
        {
            if (energyDirection == null)
                energyDirection = EnumHelper.Enumerate<EnergyDirectionType>();
            string text = searchControl ? "- Tümü -" : "- Seçiniz -";
            DropDownList ddown = searchControl ? ddlEnergyDirectionF : ddlEnergyDirection;
            ddown.DataSource = energyDirection;
            ddown.DataTextField = "Text";
            ddown.DataValueField = "Value";
            ddown.DataBind();
            ddown.Items.Insert(0, new ListItem(text, String.Empty));
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
        protected string ParseMeasuerementType(object typeId)
        {
            if (typeId == null)
                return string.Empty;
            return EnumHelper.ParseEnum<MeasurementType>(typeId).GetDisplayName();
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
        private void ListFormulation(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;
            Int32 totalRows = -1;
            if (calcPage)
            {
                grdFormulation.DataSource = Formulation.ListFormulation(startIndex, ucPager.PageSize, ListFilter, ref totalRows);
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
                grdFormulation.DataSource = Formulation.ListFormulation(startIndex, ucPager.PageSize, ListFilter);
                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grdFormulation.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {

            if (!String.IsNullOrWhiteSpace(ddlMeasurementTypeF.SelectedValue))
            {
                ListFilter.MeasurementTypeId = Convert.ToInt16(ddlMeasurementTypeF.SelectedValue.Trim());
            }
            else ListFilter.MeasurementTypeId = null;
            if (!string.IsNullOrEmpty(ddlSignF.SelectedValue))
                ListFilter.Sign = ddlSignF.SelectedValue;
            else
                ListFilter.Sign = null;
            if (!string.IsNullOrEmpty(ddlStatusF.SelectedValue))
                ListFilter.StatusTypeId = Convert.ToInt32(ddlStatusF.SelectedValue);
            else
                ListFilter.StatusTypeId = null;
            if (!string.IsNullOrEmpty(ddlEnergyDirectionF.SelectedValue))
                ListFilter.EnergyDirectionTypeId = Convert.ToInt32(ddlEnergyDirectionF.SelectedValue);
            else
                ListFilter.EnergyDirectionTypeId = null;
            ListFormulation(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveChanges.Visible = true;
            tId.Visible = true;
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;
            ddlSign.ClearSelection();
            ddlMeasurementType.ClearSelection();
            ddlStatus.ClearSelection();
            ddlEnergyDirection.ClearSelection();
            txtId.Text = string.Empty;
            txtComment.Text = string.Empty;
            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt";
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
}});", pnlAddEdit.ClientID), true);
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;
            var item = Formulation.GetFormulation(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.Visible = true;
                tId.Visible = false;
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.Id.ToString();
                ddlMeasurementType.ClearSelection();
                ddlSign.ClearSelection();
                ddlEnergyDirection.ClearSelection();
                ddlStatus.ClearSelection();
                txtId.Text = item.Id.ToString();
                ddlMeasurementType.SetSelectedVal(item.MeasurementTypeId);
                ddlSign.SetSelectedVal(item.Sign);
                ddlStatus.SetSelectedVal(item.StatusTypeId);
                ddlEnergyDirection.SetSelectedVal(item.EnergyDirectionTypeId);
                txtComment.Text = item.Comment;
                hfStatusId.Value = item.StatusHistoryId.ToString();
                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
}});", pnlAddEdit.ClientID), true);
            }
        }


        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);
            if (String.IsNullOrWhiteSpace(ddlMeasurementType.SelectedValue.Trim()))
            {
                ucAlert.Text = "Ölçüm Tipi  girilmelidir.";
                ddlMeasurementType.Focus();
                return;
            }
            if (String.IsNullOrWhiteSpace(ddlSign.SelectedValue))
            {
                ucAlert.Text = "İşaret'i girmelisiniz.";
                ddlSign.Focus();
                return;
            }
            if (string.IsNullOrEmpty(ddlEnergyDirection.SelectedValue))
            {
                ucAlert.Text = "Enerji Yönü Seçilmedi";
                ddlEnergyDirection.Focus();
                return;
            }
            if (string.IsNullOrEmpty(ddlStatus.SelectedValue))
            {
                ucAlert.Text = "Statü Seçilmedi";
                ddlStatus.Focus();
                return;
            }
          
          
            bool isEdit = (btnSave.CommandName != "A");
            EdwFormulation formulation = null;
            if (isEdit)
                formulation = Formulation.GetFormulation(Convert.ToInt32(btnSave.CommandArgument));
            else
                formulation = new EdwFormulation();
            formulation.MeasurementTypeId = Convert.ToInt16(ddlMeasurementType.SelectedValue);
            formulation.Sign = ddlSign.SelectedValue;
            formulation.StatusTypeId = Convert.ToInt32(ddlStatus.SelectedValue);
            formulation.EnergyDirectionTypeId = Convert.ToInt32(ddlEnergyDirection.SelectedValue);
            formulation.Comment = txtComment.Text;
            OlcuYonetimSistemi.DbHelper.DbResponse<EdwFormulation> saveResp = Formulation.SaveFormulation(formulation);
            
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
            var resp = Formulation.DeleteFormulation(id);
            switch (resp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.NotFound:
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    ListFormulation((Int32)ViewState["StartRowIndex"], true);
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
            var item = Formulation.GetFormulation(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
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
                ListFormulation(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListFormulation(newIndex, true);
        }
     

        
    }
}