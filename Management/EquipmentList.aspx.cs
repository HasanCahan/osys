using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class EquipmentList : System.Web.UI.Page
    {
        private Controllers.Equipment.ListFilter m_ListFilter = null;
        private Controllers.Equipment.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new Controllers.Equipment.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as Controllers.Equipment.ListFilter);
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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (IsSelectMode)
                {
                    pnlAddEdit.Visible = false;
                }
                else
                {
                    if (Request["equipmentid"] != null && Information.IsNumeric(Request["equipmentid"].ToString().Trim()))
                    {
                        int EquipmentId = Convert.ToInt32(Request["equipmentid"].ToString().Trim());
                        Models.tEquipment ekipman = Controllers.Equipment.GetEquipment(EquipmentId);
                        if (ekipman != null)
                        {
                            txtEquipmentIdF.Text = ekipman.EquipmentId.ToString();
                            txtEquipmentNameF.Text = ekipman.EquipmentName;
                            btnSearch_Click(btnSearch, e);
                            return;
                        }
                    }
                }
                ListEquipment(1, true);
            }
        }

        private void ListEquipment(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;

            Int32 totalRows = -1;
            if (calcPage)
            {
                grdEquipment.DataSource = Controllers.Equipment.ListEquipment(startIndex, ucPager.PageSize, ListFilter, ref totalRows);

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
                grdEquipment.DataSource = Controllers.Equipment.ListEquipment(startIndex, ucPager.PageSize, ListFilter);

                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grdEquipment.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(ddlEquipmentTypeF.SelectedValue))
            {
                ListFilter.Type = ddlEquipmentTypeF.SelectedValue.Trim();
            }
            else ListFilter.Type = null;

            if (!String.IsNullOrWhiteSpace(txtEquipmentIdF.Text))
            {
                ListFilter.EquipmentId = Convert.ToInt32(Information.IsNumeric(txtEquipmentIdF.Text.Trim()) ? txtEquipmentIdF.Text.Trim() : "0");
            }
            else ListFilter.EquipmentId = null;

            if (!String.IsNullOrWhiteSpace(txtEquipmentNameF.Text))
            {
                ListFilter.Name = txtEquipmentNameF.Text.Trim();
                if (ListFilter.Name.IndexOf('%') < 0) ListFilter.Name = '%' + ListFilter.Name + '%';
            }
            else ListFilter.Name = null;

            if (!string.IsNullOrWhiteSpace(txtEquipmentRefF.Text))
            {
                ListFilter.RefText = txtEquipmentRefF.Text.Trim();
            }
            else ListFilter.RefText = null;

            /*if (!String.IsNullOrWhiteSpace(txtCBSIdF.Text))
            {
                ListFilter.CBSRef = Convert.ToInt32(Information.IsNumeric(txtCBSIdF.Text.Trim()) ? txtCBSIdF.Text.Trim() : "0");
            }
            else ListFilter.CBSRef = null;*/

            ListEquipment(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;
            txtEquipmentId.Text = String.Empty;
            ddlEquipmentType.ClearSelection();
            cbBidirectional.Checked = false;
            txtEquipmentName.Text = String.Empty;
            txtEquipmentRef.Text = String.Empty;
            txtCBSId.Text = String.Empty;

            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt";
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;

            Models.tEquipment item = Controllers.Equipment.GetEquipment(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.EquipmentId.ToString();

                txtEquipmentId.Text = item.EquipmentId.ToString();
                ddlEquipmentType.ClearSelection(); ddlEquipmentType.SelectedValue = item.EquipmentType;
                cbBidirectional.Checked = item.Bidirectional.GetValueOrDefault(false);
                txtEquipmentName.Text = item.EquipmentName;
                txtEquipmentRef.Text = item.EquipmentRefText ?? (item.EquipmentRefNo.HasValue ? item.EquipmentRefNo.Value.ToString() : String.Empty);
                txtCBSId.Text = item.CBSId.HasValue ? item.CBSId.Value.ToString() : String.Empty;

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);

            if (String.IsNullOrWhiteSpace(txtEquipmentName.Text))
            {
                ucAlert.Text = "Ekipman Adı girmelisiniz.";
                txtEquipmentName.Focus();
                return;
            }
            if (ddlEquipmentType.SelectedIndex == 0)
            {
                ucAlert.Text = "Ekipman Tipini seçmelisiniz.";
                ddlEquipmentType.Focus();
                return;
            }
            if (ddlEquipmentType.SelectedValue != "H" && String.IsNullOrWhiteSpace(txtEquipmentRef.Text))
            {
                ucAlert.Text = "Referans Kodu girmelisiniz.";
                txtEquipmentRef.Focus();
                return;
            }
            if (!String.IsNullOrWhiteSpace(txtCBSId.Text) && !Information.IsNumeric(txtCBSId.Text.Trim()))
            {
                ucAlert.Text = "CBS Referans No yalnızca sayısal olabilir.";
                txtCBSId.Focus();
                return;
            }

            bool isEdit = (btnSave.CommandName == "A" ? false : true);
            Models.tEquipment ekipman = new Models.tEquipment()
            {
                EquipmentId = isEdit ? Convert.ToInt32(btnSave.CommandArgument) : -1,
                EquipmentType = ddlEquipmentType.SelectedValue,
                EquipmentName = txtEquipmentName.Text.Trim(),
                EquipmentRefText = txtEquipmentRef.Text.Trim(),
                EquipmentRefNo = Information.IsNumeric(txtEquipmentRef.Text.Trim()) ? Convert.ToInt64(txtEquipmentRef.Text.Trim()) : new Nullable<long>(),
                CBSId = (txtCBSId.Text.Trim().Length > 0) ? Convert.ToInt32(txtCBSId.Text.Trim()) : new Nullable<int>(),
                Bidirectional = cbBidirectional.Checked
            };

            using (TransactionScope scope = new TransactionScope())
            {
                DbHelper.DbResponse<Models.tEquipment> saveResp = Controllers.Equipment.SaveEquipment(ekipman);
                if (saveResp.StatusCode == DbHelper.DbResponseStatus.Conflict)
                {
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                }
                else if (saveResp.StatusCode == DbHelper.DbResponseStatus.OK)
                {
                    bool cancelled = false;

                    ekipman = saveResp.Data;
                    if (isEdit)
                    {
                        if (!cancelled && ekipman.EquipmentType != "H")
                        { //Hesaplama tipi değilse paramatreleri silelim
                            DbHelper.DbResponse resp = Controllers.CalcParam.DeleteCalcParamByEquipment(ekipman.EquipmentId);
                            switch (resp.StatusCode)
                            {
                                case DbHelper.DbResponseStatus.NotFound:
                                case DbHelper.DbResponseStatus.OK:
                                    break;
                                default:
                                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                                    ucAlert.Text = resp.StatusDescription ?? "İlişkili parametreler silinemediği için işlem tamamlanamıyor.";
                                    cancelled = true;
                                    break;
                            }
                        }

                        if (!cancelled && !cbBidirectional.Checked)
                        { //Çift yönlü değilse Output manuel verilerini silelim.
                            DbHelper.DbResponse resp = Controllers.Readout.DeleteReadoutByEquipment(ekipman.EquipmentId, "O");
                            switch (resp.StatusCode)
                            {
                                case DbHelper.DbResponseStatus.NotFound:
                                case DbHelper.DbResponseStatus.OK:
                                    break;
                                default:
                                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                                    ucAlert.Text = resp.StatusDescription ?? "İlişkili B (Output) manuel verileri silinemediği için işlem tamamlanamıyor.";
                                    cancelled = true;
                                    break;
                            }
                        }

                        if (!cancelled && !cbBidirectional.Checked)
                        { //Çift yönlü değilse Output ölçüm noktası kayıtlarını silelim.
                            DbHelper.DbResponse resp = Controllers.MeterPoint.DeleteMeterPointByEquipment(ekipman.EquipmentId, "O");
                            switch (resp.StatusCode)
                            {
                                case DbHelper.DbResponseStatus.NotFound:
                                case DbHelper.DbResponseStatus.OK:
                                    break;
                                default:
                                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                                    ucAlert.Text = resp.StatusDescription ?? "İlişkili B (Output) ölçüm noktaları silinemediği için işlem tamamlanamıyor.";
                                    cancelled = true;
                                    break;
                            }
                        }
                    }

                    if (!cancelled)
                    {
                        scope.Complete();

                        lbRefreshPage.CommandArgument = ucPager.CurrentPage.ToString();
                        lbRefreshPage.CommandName = (btnSave.CommandName == "A" ? "CALCPAGE" : String.Empty);
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

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument) || !Information.IsNumeric(btnDelete.CommandArgument)) return;

            DbHelper.DbResponse resp = Controllers.Equipment.DeleteEquipment(Convert.ToInt32(btnDelete.CommandArgument.Trim()));
            switch (resp.StatusCode)
            {
                case DbHelper.DbResponseStatus.NotFound:
                case DbHelper.DbResponseStatus.OK:
                    ListEquipment((Int32)ViewState["StartRowIndex"], true);
                    break;
                default:
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsNotify", Helper.GetNotifyJS(Helper.NotifyJSType.Danger, resp.StatusDescription, true), true);
                    break;
            }
        }

        protected void btnSelectItem_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            if (String.IsNullOrEmpty(SelectCallback)) return; //geri çağrılacak metod yok
            Button btnSelectItem = (sender as Button);
            Models.tEquipment item = Controllers.Equipment.GetEquipment(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
            if (item != null)
            {
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", String.Format("$(function() {{ window.parent.{0}({1}); }});", SelectCallback, ser.Serialize(item)), true);
            }
        }

        protected string GetDialogUrl(int equipmentId, string command)
        {
            string title = "";
            string url = "";
            UriBuilder uri = null;
            switch ((command ?? String.Empty).ToUpper())
            {
                case "READOUT":
                    uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/ReadoutList.aspx")));
                    uri.Query = "equipmentid=" + equipmentId.ToString();
                    return uri.ToString();
                case "PARAM":
                    title = "Hesaplama Parametreleri";
                    uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/CalcParamAddEdit.aspx")));
                    uri.Query = "equipmentid=" + equipmentId.ToString();
                    url = uri.ToString();
                    break;
            }
            return String.Format("javascript:showModal('{0}', '{1}')", HttpUtility.JavaScriptStringEncode(title, false), HttpUtility.JavaScriptStringEncode(url, false));
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
                ListEquipment(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListEquipment(newIndex, true);
        }
    }
}