using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class ReadoutList : System.Web.UI.Page
    {
        private Controllers.Readout.ListFilter m_ListFilter = null;
        private Controllers.Readout.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new Controllers.Readout.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as Controllers.Readout.ListFilter);
                    }
                }
                return m_ListFilter;
            }
        }

        private const string dateFormat = "dd.MM.yyyy";   //"dd.MM.yyyy hh:mm"

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                //ddlCityF.DataSource = Controllers.CityTown.ListCity();
                //ddlCityF.DataBind();
                //ddlCityF.Items.Insert(0, new ListItem("- Tümü -", String.Empty));
                //ddlCityF_SelectedIndexChanged(ddlCityF, e);

                if (Request["equipmentid"] != null && Information.IsNumeric(Request["equipmentid"].ToString().Trim()))
                {
                    int EquipmentId = Convert.ToInt32(Request["equipmentid"].ToString().Trim());
                    Models.tEquipment ekipman = Controllers.Equipment.GetEquipment(EquipmentId);
                    if (ekipman != null)
                    {
                        hfEquipmentIdF.Value = ekipman.EquipmentId.ToString();
                        txtEquipmentF.Text = ekipman.EquipmentName;
                        btnSearch_Click(btnSearch, e);
                        return;
                    }
                }

                ListReadout(1, true);
            }
        }

        private void ListReadout(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;

            Int32 totalRows = -1;
            if (calcPage)
            {
                grReadout.DataSource = Controllers.Readout.ListReadout(startIndex, ucPager.PageSize, ListFilter, ref totalRows);

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
                grReadout.DataSource = Controllers.Readout.ListReadout(startIndex, ucPager.PageSize, ListFilter);

                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grReadout.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            /*
            if (!String.IsNullOrWhiteSpace(ddlCityF.SelectedValue))
            {
                ListFilter.City = Convert.ToInt32(ddlCityF.SelectedValue);
            }
            else ListFilter.City = null;

            if (!String.IsNullOrWhiteSpace(ddlTownF.SelectedValue))
            {
                ListFilter.Town = Convert.ToInt32(ddlTownF.SelectedValue);
            }
            else ListFilter.Town = null;

            if (Information.IsNumeric(hfMeteredAreaIdF.Value.Trim()))
            {
                ListFilter.MeteredArea = Convert.ToInt32(hfMeteredAreaIdF.Value.Trim());
            }
            else ListFilter.MeteredArea = null;*/

            if (Information.IsNumeric(hfEquipmentIdF.Value.Trim()))
            {
                ListFilter.Equipment = Convert.ToInt32(hfEquipmentIdF.Value.Trim());
            }
            else ListFilter.Equipment = null;
            if (ddlEquipmentTypeF.SelectedIndex > 0)
            {
                ListFilter.EquipmentType = ddlEquipmentTypeF.SelectedValue;
            }
            else ListFilter.EquipmentType = null;
            /*
            if (ddlReadSourceF.SelectedIndex > 0)
            {
                ListFilter.ReadSource = ddlReadSourceF.SelectedValue.Trim();
            }
            else ListFilter.ReadSource = null;*/

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

            if (!String.IsNullOrWhiteSpace(txtReadEndDateF.Text))
            {
                DateTime tmp;
                if (DateTime.TryParseExact(txtReadEndDateF.Text.Trim(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tmp))
                {
                    ListFilter.EndDate = tmp;
                }
                else ListFilter.EndDate = null;
            }
            else ListFilter.EndDate = null;

            if (!String.IsNullOrWhiteSpace(txtDescriptionF.Text))
            {
                ListFilter.Description = txtDescriptionF.Text.Trim();
                if (ListFilter.Description.IndexOf('%') < 0) ListFilter.Description = '%' + ListFilter.Description + '%';
            }
            else ListFilter.Description = null;

            ListReadout(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;

            txtReadoutId.Text = String.Empty;
            //hfMeteredAreaId.Value = String.Empty;
            //txtMeteredArea.Text = String.Empty;
            hfEquipmentId.Value = String.Empty;
            txtEquipment.Text = String.Empty;
            ddlReadSource.ClearSelection();
            ddlReadSource.Items.FindByValue("O").Enabled = true;
            txtReadBeginDate.Text = String.Empty;
            txtReadEndDate.Text = String.Empty;
            txtkWh.Text = String.Empty;
            txtDescription.Text = String.Empty;
            cbTemporary.Checked = false;

            if (ListFilter.Equipment.HasValue)
            { //Filtrelenen ekipmana kolayca yeni kayıt açmak için
                Models.tEquipment ekipman = Controllers.Equipment.GetEquipment(ListFilter.Equipment.Value);
                if (ekipman != null)
                {
                    hfEquipmentId.Value = ekipman.EquipmentId.ToString();
                    txtEquipment.Text = ekipman.EquipmentName;
                    ddlReadSource.Items.FindByValue("O").Enabled = ekipman.Bidirectional.GetValueOrDefault(false);
                }
            }

            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt";
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
$('#txtReadBeginDate, #txtReadEndDate').datetimepicker(dtpProp);
}});", pnlAddEdit.ClientID), true);
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;

            Models.tReadout item = Controllers.Readout.GetReadout(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.ReadoutId.ToString();

                txtReadoutId.Text = item.ReadoutId.ToString();
                hfEquipmentId.Value = item.EquipmentId.ToString();
                txtEquipment.Text = item.EquipmentName;
                ddlReadSource.ClearSelection();
                ddlReadSource.Items.FindByValue("O").Enabled = true;
                ddlReadSource.SelectedValue = item.ReadSource;
                txtReadBeginDate.Text = item.ReadBeginDate.ToString(dateFormat);
                txtReadEndDate.Text = item.ReadEndDate.ToString(dateFormat);
                txtkWh.Text = item.kWh.ToString();
                txtDescription.Text = item.Description;
                cbTemporary.Checked = item.IsTemporary.GetValueOrDefault(false);

                Models.tEquipment ekipman = Controllers.Equipment.GetEquipment(item.EquipmentId);
                if (ekipman != null)
                {
                    ddlReadSource.Items.FindByValue("O").Enabled = ekipman.Bidirectional.GetValueOrDefault(false);
                }

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
$('#txtReadBeginDate, #txtReadEndDate').datetimepicker(dtpProp);
}});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);

            if (!Information.IsNumeric(hfEquipmentId.Value) || String.IsNullOrWhiteSpace(txtEquipment.Text))
            {
                ucAlert.Text = "Ekipman seçmelisiniz.";
                txtEquipment.Focus();
                return;
            }

            if (ddlReadSource.SelectedIndex == 0)
            {
                ucAlert.Text = "Veri Kaynağı seçmelisiniz.";
                ddlReadSource.Focus();
                return;
            }

            if (ddlReadSource.SelectedValue == "O")
            {
                Models.tEquipment ekipman = Controllers.Equipment.GetEquipment(Convert.ToInt32(hfEquipmentId.Value));
                if (ekipman != null && ekipman.Bidirectional.GetValueOrDefault(false) == false)
                {
                    ucAlert.Text = "B (Output) Veri Kaynağı sadece çift yönlü ekipmanlar için geçerlidir.";
                    ddlReadSource.Focus();
                    return;
                }
                else if (ekipman == null)
                {
                    ucAlert.Text = "Seçilen ekipmana erişilemiyor.";
                    txtEquipment.Focus();
                    return;
                }
            }

            DateTime readBeginDate;
            if (DateTime.TryParseExact(txtReadBeginDate.Text.Trim(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out readBeginDate) == false)
            {
                ucAlert.Text = "Başlangıç tarihini giriniz.";
                txtReadBeginDate.Focus();
                return;
            }
            DateTime readEndDate;
            if (DateTime.TryParseExact(txtReadEndDate.Text.Trim(), dateFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out readEndDate) == false)
            {
                ucAlert.Text = "Bitiş tarihini giriniz.";
                txtReadEndDate.Focus();
                return;
            }
            if (readBeginDate.Date > readEndDate.Date)
            {
                ucAlert.Text = "Başlangış tarihi, bitiş tarihinden sonra olamaz.";
                txtReadEndDate.Focus();
                return;
            }
            /*if (readBeginDate.Date != readEndDate.Date)
            {
                ucAlert.Text = "Başlangış ve bitiş tarihi farklı günler olamaz.";
                txtReadEndDate.Focus();
                return;
            }*/

            if (readEndDate.Subtract(readBeginDate).TotalDays > 366)
            {
                ucAlert.Text = "Başlangış ve bitiş tarihi arasında 1 yıldan uzun fark olamaz.";
                txtReadEndDate.Focus();
                return;
            }

            if (String.IsNullOrWhiteSpace(txtkWh.Text) || Helper.ConvertDouble(txtkWh.Text).GetValueOrDefault(0) == 0)
            {
                ucAlert.Text = "Geçerli bir enerji miktarı giriniz.";
                txtkWh.Focus();
                return;
            }

            if (String.IsNullOrWhiteSpace(txtDescription.Text))
            {
                ucAlert.Text = "Açıklama giriniz.";
                txtDescription.Focus();
                return;
            }

            Models.tReadout readout = new Models.tReadout()
            {
                ReadoutId = btnSave.CommandName == "A" ? -1 : Convert.ToInt32(btnSave.CommandArgument),
                EquipmentId = Convert.ToInt32(hfEquipmentId.Value),
                ReadSource = ddlReadSource.SelectedValue,
                ReadBeginDate = readBeginDate,
                ReadEndDate = readEndDate,
                kWh = Helper.ConvertDouble(txtkWh.Text).Value,
                Description = txtDescription.Text.Trim(),
                IsTemporary = cbTemporary.Checked
            };
            int conflictId = Controllers.Readout.CheckConflict(readout.EquipmentId, readout.ReadBeginDate, readout.ReadEndDate);

            if (conflictId > 0 && conflictId != readout.ReadoutId)
            {
                ucAlert.Text = "Girdiğiniz tarihler başka bir düzeltme kaydı ile çakışıyor.";
            }
            else
            {
                DbHelper.DbResponse<Models.tReadout> saveResp = Controllers.Readout.SaveReadout(readout);
                switch (saveResp.StatusCode)
                {
                    case DbHelper.DbResponseStatus.OK:
                        lbRefreshPage.CommandArgument = ucPager.CurrentPage.ToString();
                        lbRefreshPage.CommandName = (btnSave.CommandName == "A" ? "CALCPAGE" : String.Empty);
                        upAddEdit.Update();

                        ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsAddEdit"
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
        }

        /*
        protected void ddlCityF_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTownF.ClearSelection();
            ddlTownF.Items.Clear();

            ddlTownF.DataSource = ddlCityF.SelectedIndex > 0 ? Controllers.CityTown.ListTown(Convert.ToInt32(ddlCityF.SelectedValue)) : null;
            ddlTownF.DataBind();
            ddlTownF.Items.Insert(0, new ListItem("- Tümü -", String.Empty));
        }*/

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument) || !Information.IsNumeric(btnDelete.CommandArgument)) return;

            DbHelper.DbResponse resp = Controllers.Readout.DeleteReadout(Convert.ToInt32(btnDelete.CommandArgument.Trim()));
            switch (resp.StatusCode)
            {
                case DbHelper.DbResponseStatus.NotFound:
                case DbHelper.DbResponseStatus.OK:
                    ListReadout((Int32)ViewState["StartRowIndex"], true);
                    break;
                default:
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, resp.StatusDescription, true), true);
                    break;
            }
        }

        /*
        protected void lbSrcMeteredArea_Click(object sender, EventArgs e)
        {
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Ölçüm Sahası Seçimi','/Management/MeteredAreaList.aspx?wmode=select&callback=SelectMeteredArea');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }

        protected void lbRmvMeteredAreaF_Click(object sender, EventArgs e)
        {
            hfMeteredAreaIdF.Value = String.Empty;
            txtMeteredAreaF.Text = String.Empty;
        }*/

        protected void lbSrcEquipment_Click(object sender, EventArgs e)
        {
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Ekipman Seçimi','/Management/EquipmentList.aspx?wmode=select&callback=SelectEquipment');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }

        protected void lbRmvEquipmentF_Click(object sender, EventArgs e)
        {
            hfEquipmentIdF.Value = String.Empty;
            txtEquipmentF.Text = String.Empty;
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
                ListReadout(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListReadout(newIndex, true);
        }
    }
}