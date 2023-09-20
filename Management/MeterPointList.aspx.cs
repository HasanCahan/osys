using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class MeterPointList : System.Web.UI.Page
    {
        private Controllers.MeterPoint.ListFilter m_ListFilter = null;
        private Controllers.MeterPoint.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new Controllers.MeterPoint.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as Controllers.MeterPoint.ListFilter);
                    }
                }
                return m_ListFilter;
            }
        }

        private const string dateFormat = "dd.MM.yyyy HH:mm";   //"dd.MM.yyyy hh:mm"        

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                ddlCityF.DataSource = Controllers.CityTown.ListCity();
                ddlCityF.DataBind();
                ddlCityF.Items.Insert(0, new ListItem("- Tümü -", String.Empty));
                ddlCityF_SelectedIndexChanged(ddlCityF, e);

                if (Request["meteredareaid"] != null && Information.IsNumeric(Request["meteredareaid"].ToString().Trim()))
                {
                    int MeteredAreaId = Convert.ToInt32(Request["meteredareaid"].ToString().Trim());
                    Models.tMeteredArea area = Controllers.MeteredArea.GetMeteredArea(MeteredAreaId);
                    if (area != null)
                    {
                        hfMeteredAreaIdF.Value = area.MeteredAreaId.ToString();
                        txtMeteredAreaF.Text = area.AreaName;
                        btnSearch_Click(btnSearch, e);
                        return;
                    }
                }

                ListMeterPoint(1, true);
            }
        }

        private void ListMeterPoint(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;

            Int32 totalRows = -1;
            if (calcPage)
            {
                if (!String.IsNullOrWhiteSpace(ddlIsValid.SelectedValue))
                {
                    ListFilter.Valid = Convert.ToInt32(ddlIsValid.SelectedValue);
                }

                grMeterPoint.DataSource = Controllers.MeterPoint.ListMeterPoint(startIndex, ucPager.PageSize, ListFilter, ref totalRows);

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
                grMeterPoint.DataSource = Controllers.MeterPoint.ListMeterPoint(startIndex, ucPager.PageSize, ListFilter);

                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grMeterPoint.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
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
            else ListFilter.MeteredArea = null;

            if (Information.IsNumeric(hfEquipmentIdF.Value.Trim()))
            {
                ListFilter.Equipment = Convert.ToInt32(hfEquipmentIdF.Value.Trim());
            }
            else ListFilter.Equipment = null;

            if (ddlReadSourceF.SelectedIndex > 0)
            {
                ListFilter.ReadSource = ddlReadSourceF.SelectedValue.Trim();
            }
            else ListFilter.ReadSource = null;

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

            if (ddlIsValid.SelectedIndex > 0)
            {
                ListFilter.Valid = Convert.ToInt32(ddlIsValid.SelectedValue.Trim());
            }
            else ListFilter.Valid = 0;

            ListMeterPoint(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnNewSaveChanges.Visible = false;
            btnSaveChanges.Visible = true;

            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;
            lbSrcMeteredArea.Enabled = true;
            txtReadBeginDate.Enabled = true;
            lbSrcEquipment.Enabled = true;
            txtMeteredArea.Enabled = true;
            ddlReadSource.Enabled = true;
            txtEquipment.Enabled = true;
            ddlCalcSign.Enabled = true;
            txtMeterPointId.Text = String.Empty;
            hfMeteredAreaId.Value = String.Empty;
            txtMeteredArea.Text = String.Empty;
            hfEquipmentId.Value = String.Empty;
            txtEquipment.Text = String.Empty;
            ddlReadSource.ClearSelection();
            ddlCalcSign.ClearSelection();
            txtReadBeginDate.Text = String.Empty;
            txtReadEndDate.Text = String.Empty;
            //
            if (ListFilter.MeteredArea.HasValue)
            { //Filtrelenen ölçüm sahasına kolayca yeni kayıt açmak için
                Models.tMeteredArea area = Controllers.MeteredArea.GetMeteredArea(ListFilter.MeteredArea.Value);
                if (area != null)
                {
                    hfMeteredAreaId.Value = area.MeteredAreaId.ToString();
                    txtMeteredArea.Text = area.AreaName;
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
            btnNewSaveChanges.Visible = false;
            btnSaveChanges.Visible = true;

            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;

            Models.tMeterPoint item = Controllers.MeterPoint.GetMeterPoint(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.MeterPointId.ToString();

                txtMeterPointId.Text = item.MeterPointId.ToString();
                hfMeteredAreaId.Value = item.MeteredAreaId.ToString();
                txtMeteredArea.Text = item.AreaName;
                hfEquipmentId.Value = item.EquipmentId.ToString();
                txtEquipment.Text = item.EquipmentName;
                ddlReadSource.ClearSelection(); ddlReadSource.SelectedValue = item.ReadSource;
                ddlCalcSign.ClearSelection(); ddlCalcSign.SelectedValue = item.CalcSign;
                txtReadBeginDate.Text = (item.BeginDate != DateTime.MinValue) ? item.BeginDate.ToString(dateFormat) : "";
                txtReadEndDate.Text = (item.EndDate != DateTime.MinValue) ? item.EndDate.ToString(dateFormat) : "";
                lbSrcMeteredArea.Enabled = true;
                txtReadBeginDate.Enabled = true;
                lbSrcEquipment.Enabled = true;
                txtMeteredArea.Enabled = true;
                ddlReadSource.Enabled = true;
                txtEquipment.Enabled = true;
                ddlCalcSign.Enabled = true;

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

            if (!Information.IsNumeric(hfMeteredAreaId.Value) || String.IsNullOrWhiteSpace(txtMeteredArea.Text))
            {
                ucAlert.Text = "Ölçüm Sahası seçmelisiniz.";
                txtMeteredArea.Focus();
                return;
            }

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

            if (ddlCalcSign.SelectedIndex == 0)
            {
                ucAlert.Text = "Hesap İşareti seçmelisiniz.";
                ddlCalcSign.Focus();
                return;
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
                //    ucAlert.Text = "Bitiş tarihini giriniz.";
                //    txtReadEndDate.Focus();
                //    return;
            }
            if (!String.IsNullOrEmpty(txtReadEndDate.Text))
            {
                if (readBeginDate.Date > readEndDate.Date)
                {
                    ucAlert.Text = "Başlangış tarihi, bitiş tarihinden sonra olamaz.";
                    txtReadEndDate.Focus();
                    return;
                }                
            }

            Models.tMeterPoint point = new Models.tMeterPoint()
            {                
                MeterPointId = btnSave.CommandName == "A" ? -1 : Convert.ToInt32(btnSave.CommandArgument),
                MeteredAreaId = Convert.ToInt32(hfMeteredAreaId.Value),
                EquipmentId = Convert.ToInt32(hfEquipmentId.Value),
                ReadSource = ddlReadSource.SelectedValue,
                CalcSign = ddlCalcSign.SelectedValue,
                BeginDate = readBeginDate,
                EndDate = !String.IsNullOrWhiteSpace(txtReadEndDate.Text) ? readEndDate : DateTime.MinValue,                
            };

            int findMeterPointId = Controllers.MeterPoint.CheckCtrlTerminateDate(point.MeterPointId, point.MeteredAreaId, point.EquipmentId,point.ReadSource);

            if (findMeterPointId == point.MeterPointId)
            {
                int conflictId = Controllers.MeterPoint.CheckConflict(point.MeteredAreaId, point.EquipmentId, point.BeginDate, point.EndDate,point.ReadSource);
                if (conflictId > 0 && conflictId != point.MeterPointId)
                {
                    ucAlert.Text = "Girdiğiniz tarihler başka bir düzeltme kaydı ile çakışıyor.";
                }
                else
                {
                    DbHelper.DbResponse<Models.tMeterPoint> saveResp = Controllers.MeterPoint.SaveMeterPoint(point);
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
            else
            {
                ucAlert.Text = "Aynı ölçüm sahasına ait sonlandırılmamış ve Aynı Ölçüm Noktasına ait aynı yönlü kayıt mevcut.Öncelikle var olan kaydı sonlandırmanız gerekmektedir.";
                return;
            }
        }

        protected void ddlCityF_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTownF.ClearSelection();
            ddlTownF.Items.Clear();

            ddlTownF.DataSource = ddlCityF.SelectedIndex > 0 ? Controllers.CityTown.ListTown(Convert.ToInt32(ddlCityF.SelectedValue)) : null;
            ddlTownF.DataBind();
            ddlTownF.Items.Insert(0, new ListItem("- Tümü -", String.Empty));
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument) || !Information.IsNumeric(btnDelete.CommandArgument)) return;

            DbHelper.DbResponse resp = Controllers.MeterPoint.DeleteMeterPoint(Convert.ToInt32(btnDelete.CommandArgument.Trim()));
            switch (resp.StatusCode)
            {
                case DbHelper.DbResponseStatus.NotFound:
                case DbHelper.DbResponseStatus.OK:
                    ListMeterPoint((Int32)ViewState["StartRowIndex"], true);
                    break;
                default:
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, resp.StatusDescription, true), true);
                    break;
            }
        }

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
        }

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

        protected string GetDialogUrl(int equipmentId, string command)
        {
            UriBuilder uri = null;
            switch ((command ?? String.Empty).ToUpper())
            {
                case "LOCATE":
                    uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/EquipmentList.aspx")));
                    uri.Query = "equipmentid=" + equipmentId.ToString();
                    return uri.ToString();
                default:
                    return "javascript:void()";
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
                ListMeterPoint(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListMeterPoint(newIndex, true);
        }

        protected void lbtnTerminateEnd_Click(object sender, EventArgs e)
        {
            btnSaveChanges.Visible = true;
            btnNewSaveChanges.Visible = false;
            if (!(sender is LinkButton)) return;
            LinkButton btnChange = (sender as LinkButton);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;

            Models.tMeterPoint item = Controllers.MeterPoint.GetMeterPoint(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.MeterPointId.ToString();
                txtMeterPointId.Text = item.MeterPointId.ToString();
                hfMeteredAreaId.Value = item.MeteredAreaId.ToString();
                txtMeteredArea.Text = item.AreaName;
                hfEquipmentId.Value = item.EquipmentId.ToString();
                txtEquipment.Text = item.EquipmentName;
                ddlReadSource.ClearSelection(); ddlReadSource.SelectedValue = item.ReadSource;
                ddlCalcSign.ClearSelection(); ddlCalcSign.SelectedValue = item.CalcSign;
                txtReadBeginDate.Text = item.BeginDate.ToString(dateFormat);
                txtReadEndDate.Text = (item.EndDate != DateTime.MinValue) ? item.EndDate.ToString(dateFormat) : "";
                if (item.BeginDate == DateTime.MinValue)
                {
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, "Başlangıç Tarihi Girilmemiş Kaydın Sonlandırma Tarihi Girilemez !", true), true);
                    return;
                }
                lbSrcMeteredArea.Enabled = false;
                txtReadBeginDate.Enabled = false;
                lbSrcEquipment.Enabled = false;
                txtMeteredArea.Enabled = false;
                ddlReadSource.Enabled = false;
                txtEquipment.Enabled = false;
                ddlCalcSign.Enabled = false;

                Models.tEquipment ekipman = Controllers.Equipment.GetEquipment(item.EquipmentId);
                if (ekipman != null)
                {
                    ddlReadSource.Items.FindByValue("O").Enabled = ekipman.Bidirectional.GetValueOrDefault(false);
                }

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Sonlandırma Tarihi";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
$('#txtReadBeginDate, #txtReadEndDate').datetimepicker(dtpProp);
}});", pnlAddEdit.ClientID), true);
            }
        }

        protected void lbtnTerminateBegin_Click(object sender, EventArgs e)
        {
            btnNewSaveChanges.Visible = true;
            btnSaveChanges.Visible = false;
            txtMeterPointId.Text = String.Empty;
            txtReadBeginDate.Text = String.Empty;
            txtReadEndDate.Text = String.Empty;

            if (!(sender is LinkButton)) return;
            LinkButton btnChange = (sender as LinkButton);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;

            Models.tMeterPoint item = Controllers.MeterPoint.GetMeterPoint(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.MeterPointId.ToString();
                hftxtMeterPointId.Value = item.MeterPointId.ToString();
                // txtMeterPointId.Text = item.MeterPointId.ToString();
                hfMeteredAreaId.Value = item.MeteredAreaId.ToString();
                txtMeteredArea.Text = item.AreaName;
                hfEquipmentId.Value = item.EquipmentId.ToString();
                txtEquipment.Text = item.EquipmentName;
                ddlReadSource.ClearSelection(); ddlReadSource.SelectedValue = item.ReadSource;
                ddlCalcSign.ClearSelection(); ddlCalcSign.SelectedValue = item.CalcSign;
                // txtReadBeginDate.Text = item.BeginDate.ToString(dateFormat);
                // txtReadEndDate.Text = (item.EndDate != DateTime.MinValue) ? item.EndDate.ToString(dateFormat) : "";

                if (item.BeginDate == DateTime.MinValue)
                {
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, "Başlangıç Tarihi Girilmemiş Kaydın Sonlandırma Tarihi Girilemez !", true), true);
                    return;
                }
                lbSrcMeteredArea.Enabled = false;
                lbSrcEquipment.Enabled = false;
                txtMeteredArea.Enabled = false;
                ddlReadSource.Enabled = false;
                txtEquipment.Enabled = false;
                ddlCalcSign.Enabled = false;
                txtReadBeginDate.Enabled = true;

                Models.tEquipment ekipman = Controllers.Equipment.GetEquipment(item.EquipmentId);
                if (ekipman != null)
                {
                    ddlReadSource.Items.FindByValue("O").Enabled = ekipman.Bidirectional.GetValueOrDefault(false);
                }

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Başlatma Tarihi";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
$('#{0}').modal('show');
$('#txtReadBeginDate, #txtReadEndDate').datetimepicker(dtpProp);
}});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnNewSaveChanges_Click(object sender, EventArgs e)
        {                        
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);

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
                // ucAlert.Text = "Bitiş tarihini giriniz.";
                // txtReadEndDate.Focus();
                // return;
            }
            if (!String.IsNullOrEmpty(txtReadEndDate.Text))
            {
                if (readBeginDate.Date > readEndDate.Date)
                {
                    ucAlert.Text = "Başlangış tarihi, bitiş tarihinden sonra olamaz.";
                    txtReadEndDate.Focus();
                    return;
                }
            }

            Models.tMeterPoint point = new Models.tMeterPoint()
            {
                // MeterPointId = !String.IsNullOrWhiteSpace(hftxtMeterPointId.Value) ? Convert.ToInt32(hftxtMeterPointId.Value) : -1,
                MeterPointId = -1,
                MeteredAreaId = Convert.ToInt32(hfMeteredAreaId.Value),
                EquipmentId = Convert.ToInt32(hfEquipmentId.Value),
                ReadSource = ddlReadSource.SelectedValue,
                CalcSign = ddlCalcSign.SelectedValue,
                BeginDate = readBeginDate,
                EndDate = !String.IsNullOrWhiteSpace(txtReadEndDate.Text) ? readEndDate : DateTime.MinValue,
                IsValid = "-1"
            };

            int findMeterPointId = Controllers.MeterPoint.CheckCtrlTerminateDate(point.MeterPointId, point.MeteredAreaId, point.EquipmentId,point.ReadSource);

            if (findMeterPointId == point.MeterPointId)
            {
                int conflictId = Controllers.MeterPoint.CheckConflict(point.MeteredAreaId, point.EquipmentId, point.BeginDate, point.EndDate,point.ReadSource);
                if (conflictId > 0 && conflictId != point.MeterPointId)
                {
                    ucAlert.Text = "Girdiğiniz tarihler başka bir düzeltme kaydı ile çakışıyor.";
                }
                else
                {
                    DbHelper.DbResponse<Models.tMeterPoint> saveResp = Controllers.MeterPoint.SaveMeterPoint(point);
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
            else
            {
                ucAlert.Text = "Aynı ekipman ve ölçüm sahasına ait sonlandırılmamış kayıt mevcut.Öncelikle var olan kaydı sonlandırmanız gerekmektedir.";
                return;
            }
        }

    }
}