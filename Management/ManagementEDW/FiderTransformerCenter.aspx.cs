using OlcuYonetimSistemi.Controllers.EDW;
using OlcuYonetimSistemi.Models.Edw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management.ManagementEDW
{
    public partial class FiderTransformerCenter : System.Web.UI.Page
    {
        private TransformerCenterFider.ListFilter m_ListFilter = null;
        private string IlId = "";
        private TransformerCenterFider.ListFilter ListFilter
        {
            get
            {
                if (m_ListFilter == null)
                {
                    if (ViewState["Filter"] == null)
                    {
                        m_ListFilter = new TransformerCenterFider.ListFilter();
                        ViewState["Filter"] = m_ListFilter;
                    }
                    else
                    {
                        m_ListFilter = (ViewState["Filter"] as TransformerCenterFider.ListFilter);
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
            if (@Request["IlId"] != null || !String.IsNullOrEmpty(@Request["IlId"]))
            {
                if (this.IsSelectMode && !String.IsNullOrWhiteSpace(Request["callback"]))
                {
                    this.IlId = Request["IlId"].ToString();
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

                if (!string.IsNullOrEmpty(IlId))
                {
                    ddlCityF.SelectedValue = IlId.ToString();
                    ListFilter.City = Convert.ToInt32(IlId);
                }
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
                grTransformerCenters.DataSource = TransformerCenterFider.ListTransformerCenterFider(startIndex, ucPager.PageSize, ListFilter, ref totalRows);
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
                grTransformerCenters.DataSource = TransformerCenterFider.ListTransformerCenterFider(startIndex, ucPager.PageSize, ListFilter);
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
            ListMeteredArea(1, true);
        }

      

       

        protected void ddlCityF_SelectedIndexChanged(object sender, EventArgs e)
        {
            ddlTownF.ClearSelection();
            ddlTownF.Items.Clear();

            ddlTownF.DataSource = ddlCityF.SelectedIndex > 0 ? Controllers.CityTown.ListTown(Convert.ToInt32(ddlCityF.SelectedValue)) : null;
            ddlTownF.DataBind();
            ddlTownF.Items.Insert(0, new ListItem("- Tümü -", String.Empty));
        }

    

     

        protected void btnSelectItem_Click(object sender, EventArgs e)
        {
            if (!(sender is Button)) return;
            if (String.IsNullOrEmpty(SelectCallback)) return; //geri çağrılacak metod yok
            Button btnSelectItem = (sender as Button);
            var item = TransformerCenterFider.GetTransformerCenterFider(Convert.ToInt32(btnSelectItem.CommandArgument.Trim()));
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