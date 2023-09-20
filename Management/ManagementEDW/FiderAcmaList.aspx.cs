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
using System.Drawing;
using OlcuYonetimSistemi.Controllers.EDW;

namespace OlcuYonetimSistemi.Management.ManagementEDW
{

    public partial class FiderAcmaList : System.Web.UI.Page
    {

        private FiderAcma.ListFilter m_ListFilter = null;
        private FiderAcma.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new FiderAcma.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as FiderAcma.ListFilter);
                    }
                }
                return m_ListFilter;
            }
        }
        protected int Ay
        {
            get
            {
                return (int)(ViewState["Ay"] ?? DateTime.Now.Month);
            }
            set
            {
                ViewState["Ay"] = value;
            }
        }

        protected int Yil
        {
            get
            {
                return (int)(ViewState["Yil"] ?? DateTime.Now.Year);
            }
            set
            {
                ViewState["Yil"] = value;
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
                
                var usr = Controllers.EDW.User.GetUser(HttpContext.Current.User.Identity.Name);
                var roles = System.Web.Security.Roles.GetRolesForUser(HttpContext.Current.User.Identity.Name);
                if (!roles.Contains("ACMA"))
                {

                }
                if (!(roles != null && (roles.Contains("EDW") || roles.Contains("ADMIN"))))
                {
                    ddlCitySearch.Visible = false;
                    lblddlCitySearch.Visible = false;

                }
                else
                {
                    ddlCitySearch.DataSource = Controllers.CityTown.ListCity();
                    ddlCitySearch.DataBind();
                    ddlCitySearch.Items.Insert(0, new ListItem("- il seçiniz -", String.Empty));
                }
                ListFiderAcma(1, true);
                initializeControls(e);
                if (IsSelectMode)
                {
                    pnlAddEdit.Visible = false;
                }
                if (Request["TransformerCenterId"] != null && Information.IsNumeric(Request["TransformerCenterId"].ToString().Trim()))
                {
                    int id = Convert.ToInt32(Request["TransformerCenterId"].ToString().Trim());
                    var tc = TransformerCenterFider.GetTransformerCenterFider(id);
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
                        return;
                    }
                }
            }
        }
        void initializeControls(EventArgs e)
        {
            //if ((!filter.IlId.HasValue) || !(roles!=null && (roles.Contains("EDW")|| roles.Contains("ADMIN" ))))
        }

        DataTable generateColumns()
        {
            DataTable dt = new DataTable();
            var max = 31;// DateTime.DaysInMonth(Yil, Ay);
            BoundField colId = new BoundField();
            colId.DataField = "Id";
            colId.HeaderText = "No";

            colId.Visible = false;
            dt.Columns.Add(colId.DataField, typeof(string));
            //  grdFiderAcma.Columns.Add(colId);
            BoundField colTC = new BoundField();
            colTC.HeaderText = "Trafo Merkezi";
            colTC.DataField = "TransformerCenterName";
            //  grdFiderAcma.Columns.Add(colTC);
            dt.Columns.Add(colTC.DataField, typeof(string));
            BoundField colFider = new BoundField();
            colFider.HeaderText = "Fider";
            colFider.DataField = "FiderName";

            dt.Columns.Add(colFider.DataField, typeof(string));
            //  grdFiderAcma.Columns.Add(colFider);

            BoundField colFiderId = new BoundField();
            colFiderId.HeaderText = "Fider";
            colFiderId.DataField = "FiderId";
            colFiderId.Visible = false;
            //  grdFiderAcma.Columns.Add(colFiderId);
            dt.Columns.Add(colFiderId.DataField, typeof(int));
            dt.Columns.Add("Total", typeof(string));

            dt.Columns.Add("Bigger", typeof(bool));
            for (int i = 1; i <= max; i++)
            {
                BoundField colN = new BoundField();
                colN.HeaderText = i.ToString();
                colN.DataField = "N" + i;
                //  grdFiderAcma.Columns.Add(colN);
                dt.Columns.Add(colN.DataField, typeof(string));
            }
            //  grdFiderAcma.Columns.Add(tmp);
            //if (field != null)
            //    grdFiderAcma.Columns.Add(field);

            BoundField colCihaz = new BoundField();
            colCihaz.HeaderText = "Cihaz";
            colCihaz.DataField = "Cihaz";

            dt.Columns.Add(colCihaz.DataField, typeof(string));

            return dt;
        }
        void setDataSource(DataSet ds)
        {

            var dt = generateColumns();
            var lst = ds.ToList<EdwFiderAcma>(null);
            if (lst == null || lst.Count == 0)
                grdFiderAcma.DataSource = new List<EdwFiderAcma>();
            else
            {
                var grouped = lst.GroupBy(s => new { s.Year, s.Month }).FirstOrDefault().OrderBy(s => s.TransformerCenterName).ThenBy(x=>x.FiderName).GroupBy(f => f.FiderId);
                foreach (var itm in grouped)
                {
                    var dr = dt.NewRow();
                    var fiderName = itm.FirstOrDefault().FiderName;
                    var fiderId = itm.FirstOrDefault().FiderId;
                    var transformerCenterName = itm.FirstOrDefault().TransformerCenterName;
                    var year = itm.FirstOrDefault().Year;
                    var month = itm.FirstOrDefault().Month;
                    var cihaz = itm.FirstOrDefault().Cihaz;
                    dr["Id"] = fiderId + "-" + year + "-" + month;
                    dr["FiderName"] = fiderName;
                    dr["FiderId"] = fiderId;
                    dr["Cihaz"] = cihaz;
                    dr["TransformerCenterName"] = transformerCenterName;
                    int i = 1;
                    int total = 0;
                    bool hasBigger4 = false;
                    foreach (var itm2 in itm)
                    {
                        dr["N" + itm2.Day] = itm2.Count.ToString();
                        if (Int32.TryParse(itm2.Count.ToString(), out int t))
                            total += t;
                        if (t >= 4)
                            hasBigger4 = true;
                        i++;
                    }
                    dr["Total"] = total == 0 ? "" : total.ToString();
                    dr["Bigger"] = hasBigger4;
                    dt.Rows.Add(dr);
                }
                DataSet ds2 = new DataSet();
                ds2.Tables.Add(dt);
                grdFiderAcma.DataSource = ds2;
                grdFiderAcma.DataBind();
            }
        }
        private void ListFiderAcma(Int32 startIndex, bool calcPage)
        {
            if (ListFilter.IlId == null && ListFilter.FiderId == null && ListFilter.TransformerCenterId == null)
                return;
            ViewState["StartRowIndex"] = startIndex;
            Int32 totalRows = -1;
            if (calcPage)
            {
                var ds = FiderAcma.ListFiderAcma(startIndex, 100000000, ListFilter, ref totalRows);
                setDataSource(ds);
            }
            else
            {
                var ds = FiderAcma.ListFiderAcma(startIndex, 100000000, ListFilter);
                setDataSource(ds);
            }
        }
        void setDate()
        {
            const string pattern = "MM.yyyy";
            bool hasDate = DateTime.TryParseExact(txtCreationDate.Text, pattern, null, System.Globalization.DateTimeStyles.None, out DateTime dt);
            if (hasDate)
            {
                Ay = dt.Month;
                Yil = dt.Year;
            }
            else
            {
                txtCreationDate.Text = DateTime.Now.ToString(pattern);
            }
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            setDate();
            if (Ay == 0)
                Ay = DateTime.Now.Month;
            if (Yil == 0)
                Yil = DateTime.Now.Year;
            ListFilter.Month = Ay;
            ListFilter.Year = Yil;
            if (!String.IsNullOrWhiteSpace(hfTransformerCenterIdF.Value))
            {
                ListFilter.TransformerCenterId = Convert.ToInt32(hfTransformerCenterIdF.Value);
            }
            else ListFilter.TransformerCenterId = null;

            if (!String.IsNullOrWhiteSpace(hfFiderIdF.Value))
            {
                ListFilter.FiderId = Convert.ToInt32(hfFiderIdF.Value);
            }
            else ListFilter.FiderId = null;

            if (!String.IsNullOrWhiteSpace(ddlCitySearch.SelectedValue))
            {
                ListFilter.IlId = Convert.ToInt32(ddlCitySearch.SelectedValue);
            }
            else
            {
                var usr = Controllers.EDW.User.GetUser(HttpContext.Current.User.Identity.Name); 
                ListFilter.IlId = usr.IlId;
            }

            ListFiderAcma(1, true);
        }


        void changeVisibility(Boolean visiblity)
        {
            dvPrimary.Visible = visiblity;
            tId.Visible = visiblity;
        }
        protected object test(object obj)
        {
            return obj;
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);

            if (String.IsNullOrEmpty(btnChange.CommandArgument)) return;
            if (grdFiderAcma.Rows.Count == 0)
                return;

            GridViewRow gvr = (GridViewRow)btnChange.NamingContainer;
            int rowindex = gvr.RowIndex;
            if (rowindex < 0)
                return;
            GridViewRow item = grdFiderAcma.Rows[rowindex];
            if (item == null)
                return;
            var itms = btnChange.CommandArgument.Split('-');
            var filter = new FiderAcma.ListFilter();
            filter.Year = Convert.ToInt32(itms[1]);
            filter.FiderId = Convert.ToInt32(itms[0]);
            filter.Month = Convert.ToInt32(itms[2]);
            var fiderAcmas = FiderAcma.ListFiderAcma(0, 1000, filter).ToList<EdwFiderAcma>(null);
            if (fiderAcmas == null || fiderAcmas.Count == 0)
                return;
            List<EdwFiderAcma> toBeUpdated = new List<EdwFiderAcma>();
            for (int i = 1; i <= 31; i++)
            {
                var val = item.FindControl("N" + i) as TextBox;
                if (val == null)
                    continue;
                if (!Int32.TryParse(val.Text, out int cnt))
                    continue;
                var foundItem = fiderAcmas.FirstOrDefault(f => f.Day == i);
                if (foundItem == null)
                    continue;

                if (cnt < 0)
                    foundItem.Count = 0;
                else
                    foundItem.Count = cnt;

                toBeUpdated.Add(foundItem);
            }
            var saveResp = FiderAcma.Update(toBeUpdated.ToArray());
            switch (saveResp.StatusCode)
            {
                case DbHelper.DbResponseStatus.OK:
                    btnSearch_Click(sender, e);
                    break;
                default:
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                    break;
            }
        }
        protected Color valueToColor(object obj)
        {
            if (obj == null)
                return Color.White;
            Int32.TryParse(obj.ToString(), out int val);
            if (val == 0)
                return Color.White;
            if (val < 4)
                return Color.Yellow;
            if (val >= 4)
                return Color.Red;
            return Color.Transparent;
        }
        protected Color valueToColorTotal(object obj, Object biggerFour)
        {
            if (obj == null)
                return Color.Transparent;
            Boolean.TryParse(biggerFour == null ? "False" : biggerFour.ToString(), out bool res);
            Int32.TryParse(obj.ToString(), out int val);
            if (val > 20)
                return Color.Red;
            else if (res && val >= 12)
                return Color.Red;
            return Color.Transparent;
        }
        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            //if (!(sender is Button)) return;
            //Button btnSave = (sender as Button);
            //var statuTypeId = Convert.ToInt32(ddlStatus.SelectedValue);
            //if (statuTypeId == 5 && string.IsNullOrEmpty(txtOsosNumber.Text) || Information.IsFindCharInNumeric(txtOsosNumber.Text.Trim()))
            //{
            //    ucAlert.Text = "Osos Tesisat Numarası Gömülü Santral için Zorunludur ";
            //    txtOsosNumber.Focus();
            //    return;
            //}
            //else if (statuTypeId != 5)
            //{
            //    txtOsosNumber.Text = "";
            //}
            //if (string.IsNullOrEmpty(txtCreationDate.Text))
            //    txtCreationDate.Text = DateTime.Now.ToString();
            //bool isEdit = (btnSave.CommandName != "A");
            //EdwFiderAcma statusH = null;
            //if (isEdit)
            //    statusH = FiderAcma.GetFiderAcma(Convert.ToInt32(btnSave.CommandArgument));
            //else
            //    statusH = new EdwFiderAcma();
            //Boolean isChangeMeasurement = btnSave.CommandName == "C";
            //if (isChangeMeasurement)
            //{
            //    statusH.Id = 0;
            //    statusH.CreationTime = DateTime.Now;
            //}
            //if (!isChangeMeasurement)
            //{
            //    statusH.StatusId = Convert.ToInt32(ddlStatus.SelectedValue);
            //    statusH.FiderId = Convert.ToInt32(hfFider.Value);
            //}
            //DateTime dt;
            //string[] pattern = { "dd/MM/yyyy hh:mm:ss", "dd.MM.yyyy hh:mm:ss", "dd/MM/yyyy", "dd.MM.yyyy", "dd/MM/yyyy hh:mm", "dd.MM.yyyy hh:mm" };
            //bool hasDate = DateTime.TryParseExact(txtCreationDate.Text, pattern, null, System.Globalization.DateTimeStyles.None, out dt);
            //if (hasDate)
            //    statusH.CreationTime = dt;
            //if (statusH.CreationTime.Date > DateTime.Now.Date)
            //{
            //    txtCreationDate.Focus();
            //    ucAlert.Text = "İşlem Tarihi bugünden büyük olamaz";
            //    return;
            //}
            //if (!string.IsNullOrEmpty(ddlEdwConsumption.SelectedValue))
            //    statusH.EdwConsumptionTypeId = Convert.ToInt32(ddlEdwConsumption.SelectedValue);
            //if (!string.IsNullOrEmpty(ddlBaraConsumption.SelectedValue))
            //    statusH.BaraConsumptionTypeId = Convert.ToInt32(ddlBaraConsumption.SelectedValue);
            //if (!string.IsNullOrEmpty(ddlFiderConsumption.SelectedValue))
            //    statusH.FiderConsumptionTypeId = Convert.ToInt32(ddlFiderConsumption.SelectedValue);
            //statusH.Comment = txtComment.Text;
            //if (!String.IsNullOrEmpty(txtOsosNumber.Text))
            //    statusH.OsosNumber = Convert.ToInt32(txtOsosNumber.Text);
            //else
            //    statusH.OsosNumber = null;
            //OlcuYonetimSistemi.DbHelper.DbResponse<EdwFiderAcma> saveResp = FiderAcma.SaveFiderAcma(statusH);
            //switch (saveResp.StatusCode)
            //{
            //    case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
            //        upAddEdit.Update();

            //        lbRefreshPage.CommandArgument = ucPager.CurrentPage.ToString();
            //        lbRefreshPage.CommandName = (btnSave.CommandName == "A" ? "CALCPAGE" : String.Empty);
            //        upAddEdit.Update();

            //        ScriptManager.RegisterClientScriptBlock(upAddEdit, upAddEdit.GetType(), "jsAddEdit"
            //            , String.Format(@"$('#{0}').modal('hide'); {1};", pnlAddEdit.ClientID
            //            , Page.ClientScript.GetPostBackEventReference(lbRefreshPage, String.Empty))
            //            , true);
            //        break;
            //    default:
            //        ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
            //        ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
            //        break;
            //}
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument)) return;
            string[] commandArgs = btnDelete.CommandArgument.ToString().Split(new char[] { ';' });
            int id = Convert.ToInt32(commandArgs[0]);
            var resp = FiderAcma.DeleteFiderAcma(id);
            switch (resp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.NotFound:
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    ListFiderAcma((Int32)ViewState["StartRowIndex"], true);
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
            var item = FiderAcma.GetFiderAcma(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
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
                ListFiderAcma(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = 1000;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListFiderAcma(newIndex, true);
        }


        protected void lbRemoveFiderF_Click(object sender, EventArgs e)
        {
            hfFiderIdF.Value = String.Empty;
            txtFiderF.Text = String.Empty;
        }

        protected void lbFiderF_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfTransformerCenterIdF.Value))
            {
                ucAlert.Text = "Trafo Seçimi için Öncelikle Trafo Merkezi seçilmelidir.";
                txtTransformerCenterF.Focus();
                return;
            }
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Trafo Seçimi','/Management/ManagementEDW/FiderList.aspx?wmode=select&callback=SelectFider&TransformerCenterId=" + hfTransformerCenterIdF.Value + "');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }
        protected void lbSrcTransformerCenter_Click(object sender, EventArgs e)
        {
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Trafo Merkezi Seçimi','/Management/ManagementEDW/FiderTransformerCenter.aspx?wmode=select&callback=SelectTransformerCenter&IlId=" + ddlCitySearch.SelectedValue.ToString()+ "');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }

        protected void lbRmvTransformerCenterCenterF_Click(object sender, EventArgs e)
        {
            hfTransformerCenterIdF.Value = String.Empty;
            txtTransformerCenterF.Text = String.Empty;
        }

        protected void lbSrcFider_Click(object sender, EventArgs e)
        {
            LinkButton lb = (sender as LinkButton);
            string js = "SelectReason = '" + lb.CommandName + "'; showModal('Trafo Seçimi','/Management/ManagementEDW/FiderList.aspx?wmode=select&callback=SelectFider');";
            if (ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
                ScriptManager.RegisterStartupScript(upAddEdit, upAddEdit.GetType(), "jsSelectItem", js, true);
            else Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", js, true);
        }

        protected void grdFiderAcma_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            var t = e.CommandArgument;
        }

        protected void grdFiderAcma_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            var t = e.CommandArgument;
        }
    }
}
