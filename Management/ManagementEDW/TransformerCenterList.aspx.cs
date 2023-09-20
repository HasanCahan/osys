using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OlcuYonetimSistemi.Controllers.EDW;
using OlcuYonetimSistemi.Models.Edw;

namespace OlcuYonetimSistemi.Management.ManagementEDW
{
    public partial class TransformerCenterList : System.Web.UI.Page
    {
        private TransformerCenter.ListFilter m_ListFilter = null;
        private TransformerCenter.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new TransformerCenter.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as TransformerCenter.ListFilter);
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
                ddlCityF.DataSource = Controllers.CityTown.ListCity();
                ddlCityF.DataBind();
                ddlCityF.Items.Insert(0, new ListItem("- Tümü -", String.Empty));
                ddlCityF_SelectedIndexChanged(ddlCityF, e);
                if (IsSelectMode) pnlAddEdit.Visible = false;
                ListMeteredArea(1, true);
            }
        }

        private void ListMeteredArea(Int32 startIndex, bool calcPage)
        {
            ViewState["StartRowIndex"] = startIndex;
            Int32 totalRows = -1;
            if (calcPage)
            {
                grTransformerCenters.DataSource = TransformerCenter.ListTransformerCenter(startIndex, ucPager.PageSize, ListFilter, ref totalRows);
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
                grTransformerCenters.DataSource = TransformerCenter.ListTransformerCenter(startIndex, ucPager.PageSize, ListFilter);
                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grTransformerCenters.DataBind();
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

            if (!String.IsNullOrWhiteSpace(txtNameF.Text))
            {
                ListFilter.Name = txtNameF.Text.Trim();
                if (ListFilter.Name.IndexOf('%') < 0) ListFilter.Name = '%' + ListFilter.Name + '%';
            }
            else ListFilter.Name = null;

            if (!String.IsNullOrWhiteSpace(txtEdwIdF.Text))
            {
                if (!Information.IsFindCharInNumeric(txtEdwIdF.Text))
                {
                    ListFilter.EdwNumber = Convert.ToInt32(txtEdwIdF.Text.Trim());
                }
                else ListFilter.EdwNumber = -1;
            }
            else ListFilter.EdwNumber = null;

            ListMeteredArea(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;
            txtEdwNumber.ReadOnly = false;
            ddlCity.ClearSelection();
            ddlCity.Items.Clear();
            ddlCity.DataSource = Controllers.CityTown.ListCity();
            ddlCity.DataBind();
            ddlCity.Items.Insert(0, new ListItem("- Seçiniz -", String.Empty));
            ddlCity_SelectedIndexChanged(ddlCity, e);
            txtId.Text = String.Empty;
            txtEdwNumber.Text = String.Empty;
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

            var item = TransformerCenter.GetTransformerCenter(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.Id.ToString();
                ddlCity.ClearSelection();
                ddlCity.Items.Clear();
                ddlCity.DataSource = Controllers.CityTown.ListCity();
                ddlCity.DataBind();
                ddlCity.Items.Insert(0, new ListItem("- Seçiniz -", String.Empty));
                if (item.CityId.HasValue && item.CityId != 0) ddlCity.SelectedValue = item.CityId.Value.ToString();
                ddlCity_SelectedIndexChanged(ddlCity, e);
                if (item.CityId.HasValue && item.CityId != 0) ddlTown.SelectedValue = item.TownId.ToString();
                txtId.Text = item.Id.ToString();
                txtEdwNumber.Text = item.EdwNumber.ToString();
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

            if (String.IsNullOrWhiteSpace(txtEdwNumber.Text) || Information.IsFindCharInNumeric(txtEdwNumber.Text))
            {
                ucAlert.Text = "EDW Id girmelisiniz.";
                txtEdwNumber.Focus();
                return;
            }

            if (String.IsNullOrWhiteSpace(txtName.Text))
            {
                ucAlert.Text = "Trafo Merkezi Adı girmelisiniz.";
                txtName.Focus();
                return;
            }
            if (string.IsNullOrEmpty(ddlCity.SelectedValue.Trim()))
            {
                ucAlert.Text = "Şehir Seçmelisiniz";
                ddlCity.Focus();
                return;
            }

            EdwTransformerCenter transformerCenter = new EdwTransformerCenter()
            {
                Id = btnSave.CommandName == "A" ? 0 : Convert.ToInt32(btnSave.CommandArgument),
                EdwNumber = Convert.ToInt32(txtEdwNumber.Text.Trim()),
                CityId = !String.IsNullOrEmpty(ddlCity.SelectedValue.Trim()) ? Convert.ToInt32(ddlCity.SelectedValue.Trim()) : 0,
                TownId = !String.IsNullOrEmpty(ddlTown.SelectedValue.Trim()) ? Convert.ToInt32(ddlTown.SelectedValue.Trim()) : 0,
                Name = txtName.Text.Trim(),
                IsActive = true
            };

            OlcuYonetimSistemi.DbHelper.DbResponse<EdwTransformerCenter> saveResp = TransformerCenter.SaveTransformerCenter(transformerCenter);
            switch (saveResp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
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

        protected void ddlCityF_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTownF.ClearSelection();
            ddlTownF.Items.Clear();

            ddlTownF.DataSource = ddlCityF.SelectedIndex > 0 ? Controllers.CityTown.ListTown(Convert.ToInt32(ddlCityF.SelectedValue)) : null;
            ddlTownF.DataBind();
            ddlTownF.Items.Insert(0, new ListItem("- Tümü -", String.Empty));
        }

        protected void ddlCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTown.ClearSelection();
            ddlTown.Items.Clear();

            ddlTown.DataSource = ddlCity.SelectedIndex > 0 ? Controllers.CityTown.ListTown(Convert.ToInt32(ddlCity.SelectedValue)) : null;
            ddlTown.DataBind();
            ddlTown.Items.Insert(0, new ListItem("- Seçiniz -", String.Empty));
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnDelete = (sender as Button);
            if (String.IsNullOrEmpty(btnDelete.CommandArgument) || !Information.IsNumeric(btnDelete.CommandArgument)) return;
            OlcuYonetimSistemi.DbHelper.DbResponse resp = TransformerCenter.DeleteTransformerCenter(Convert.ToInt32(btnDelete.CommandArgument.Trim()));
            switch (resp.StatusCode)
            {
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.NotFound:
                case OlcuYonetimSistemi.DbHelper.DbResponseStatus.OK:
                    ListMeteredArea((Int32)ViewState["StartRowIndex"], true);
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
            var item = TransformerCenter.GetTransformerCenter(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
            if (item != null)
            {
                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", String.Format("$(function() {{ window.parent.{0}({1}); }});", SelectCallback, ser.Serialize(item)), true);
            }
        }

        protected string GetDeviceList(int tcenterId)
        {
            UriBuilder uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/ManagementEDW/DeviceList.aspx")));
            uri.Query = "transformerCenterId=" + tcenterId.ToString();
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
                ListMeteredArea(newIndex, calcPage);
            }
        }

        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListMeteredArea(newIndex, true);
        }

    }
}