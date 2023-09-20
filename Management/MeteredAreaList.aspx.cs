using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class MeteredAreaList : System.Web.UI.Page
    {
        private Controllers.MeteredArea.ListFilter m_ListFilter = null;
        private Controllers.MeteredArea.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new Controllers.MeteredArea.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as Controllers.MeteredArea.ListFilter);
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
                grMeteredArea.DataSource = Controllers.MeteredArea.ListMeteredArea(startIndex, ucPager.PageSize, ListFilter, ref totalRows);

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
                grMeteredArea.DataSource = Controllers.MeteredArea.ListMeteredArea(startIndex, ucPager.PageSize, ListFilter);

                Dictionary<int, int> pagingIndex = (Dictionary<int, int>)ViewState["PagingIndex"];
                ucPager.CurrentPage = pagingIndex.Where(x => x.Value == startIndex).FirstOrDefault().Key;
                ucPager.DataBind();
            }
            grMeteredArea.DataBind();
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

            if (!String.IsNullOrWhiteSpace(txtDescriptionF.Text))
            {
                ListFilter.Description = txtDescriptionF.Text.Trim();
                if (ListFilter.Description.IndexOf('%') < 0) ListFilter.Description = '%' + ListFilter.Description + '%';
            }
            else ListFilter.Description = null;

            ListMeteredArea(1, true);
        }

        protected void btnNew_Click(object sender, EventArgs e)
        {
            btnSaveChanges.CommandName = "A";
            btnSaveChanges.CommandArgument = String.Empty;

            ddlCity.ClearSelection();
            ddlCity.Items.Clear();
            ddlCity.DataSource = Controllers.CityTown.ListCity();
            ddlCity.DataBind();
            ddlCity.Items.Insert(0, new ListItem("- Seçiniz -", String.Empty));
            ddlCity_SelectedIndexChanged(ddlCity, e);

            txtMeteredAreaId.Text = String.Empty;
            txtAreaName.Text = String.Empty;
            txtDescription.Text = String.Empty;

            upAddEdit.Visible = true;
            ltAddEditName.Text = "Yeni Kayıt";
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
        }

        protected void btnChange_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnChange = (sender as Button);
            if (String.IsNullOrEmpty(btnChange.CommandArgument) || !Information.IsNumeric(btnChange.CommandArgument)) return;

            Models.tMeteredArea item = Controllers.MeteredArea.GetMeteredArea(Convert.ToInt32(btnChange.CommandArgument.Trim()));
            if (item != null)
            {
                btnSaveChanges.CommandName = "E";
                btnSaveChanges.CommandArgument = item.MeteredAreaId.ToString();

                ddlCity.ClearSelection();
                ddlCity.Items.Clear();
                ddlCity.DataSource = Controllers.CityTown.ListCity();
                ddlCity.DataBind();
                ddlCity.Items.Insert(0, new ListItem("- Seçiniz -", String.Empty));
                if (item.CityId.HasValue) ddlCity.SelectedValue = item.CityId.Value.ToString();
                ddlCity_SelectedIndexChanged(ddlCity, e);
                ddlTown.SelectedValue = item.TownId.ToString();

                txtMeteredAreaId.Text = item.MeteredAreaId.ToString();
                txtAreaName.Text = item.AreaName;
                txtDescription.Text = item.Description;

                upAddEdit.Visible = true;
                ltAddEditName.Text = "Kayıt Değiştir";
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format("$(function() {{ $('#{0}').modal('show'); }});", pnlAddEdit.ClientID), true);
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            Button btnSave = (sender as Button);

            if (ddlCity.SelectedIndex == 0)
            {
                ucAlert.Text = "İl seçmelisiniz.";
                ddlCity.Focus();
                return;
            }

            if (ddlTown.SelectedIndex == 0)
            {
                ucAlert.Text = "İlçe seçmelisiniz.";
                ddlTown.Focus();
                return;
            }

            if (String.IsNullOrWhiteSpace(txtAreaName.Text))
            {
                ucAlert.Text = "Saha Adı girmelisiniz.";
                txtAreaName.Focus();
                return;
            }

            Models.tMeteredArea saha = new Models.tMeteredArea()
            {
                MeteredAreaId = btnSave.CommandName == "A" ? -1 : Convert.ToInt32(btnSave.CommandArgument),
                CityId = null,
                TownId = Convert.ToInt32(ddlTown.SelectedValue.Trim()),
                AreaName = txtAreaName.Text.Trim(),
                Description = txtDescription.Text.Trim()
            };

            DbHelper.DbResponse<Models.tMeteredArea> saveResp = Controllers.MeteredArea.SaveMeteredArea(saha);
            switch (saveResp.StatusCode)
            {
                case DbHelper.DbResponseStatus.OK:
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

            DbHelper.DbResponse resp = Controllers.MeteredArea.DeleteMeteredArea(Convert.ToInt32(btnDelete.CommandArgument.Trim()));
            switch (resp.StatusCode)
            {
                case DbHelper.DbResponseStatus.NotFound:
                case DbHelper.DbResponseStatus.OK:
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
            Models.tMeteredArea item = Controllers.MeteredArea.GetMeteredArea(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
            if (item != null)
            {

                System.Web.Script.Serialization.JavaScriptSerializer ser = new System.Web.Script.Serialization.JavaScriptSerializer();
                Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsSelectItem", String.Format("$(function() {{ window.parent.{0}({1}); }});", SelectCallback, ser.Serialize(item)), true);
            }
        }

        protected string GetMeterPointUrl(int meteredAreaId)
        {
            UriBuilder uri = new UriBuilder(new Uri(Page.Request.Url, Page.ResolveUrl("~/Management/MeterPointList.aspx")));
            uri.Query = "meteredareaid=" + meteredAreaId.ToString();
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