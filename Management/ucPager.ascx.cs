using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OlcuYonetimSistemi.Management.ManagementEDW;
namespace OlcuYonetimSistemi.Management
{
    public partial class ucPager : System.Web.UI.UserControl
    {
        private int PageBlockCount = 10;

        [Browsable(true), Description("Sayfa numarasına tıklanması")]
        public event CommandEventHandler PageNumberClick;

        [Browsable(true), Description("Sayfa boyutunun değiştirilmesi")]
        public event CommandEventHandler PageSizeChanged;

        public override void DataBind()
        {
            if (this.TotalPage < 1)
            {
                rptPaging.DataSource = null;
                rptPaging.DataBind();
                return;
            };

            List<int> allPages = new byte[this.TotalPage].Select((value, inx) => inx + 1).ToList();
            List<int> numbers = new List<int>();
            int totalBlock = (int)Math.Ceiling((float)this.TotalPage / (float)PageBlockCount);
            if (totalBlock > 1)
            { //birden fazla sayfalama bloğu varsa
                int activeBlock = (int)Math.Ceiling((float)this.CurrentPage / (float)this.PageBlockCount); //mevcut sayfanın bloğunu bulalım
                if (activeBlock == 1)
                { //başta
                    numbers = allPages.GetRange(0, this.PageBlockCount).ToList();
                    numbers.Add(int.MaxValue);
                    numbers.Add((activeBlock * this.PageBlockCount) + 1);
                }
                else if (activeBlock > 1 && activeBlock < totalBlock)
                { //ortada
                    numbers.Add((activeBlock - 1) * this.PageBlockCount);
                    numbers.Add(int.MinValue);
                    numbers.AddRange(allPages.Where(x => x > ((activeBlock - 1) * this.PageBlockCount) && x <= (activeBlock * this.PageBlockCount)).ToList());
                    numbers.Add(int.MaxValue);
                    numbers.Add((activeBlock * this.PageBlockCount) + 1);
                }
                else if (activeBlock == totalBlock)
                { //sonda
                    numbers.Add((activeBlock - 1) * this.PageBlockCount);
                    numbers.Add(int.MinValue);
                    numbers.AddRange(allPages.Where(x => x > ((activeBlock - 1) * this.PageBlockCount)).ToList());
                }
            }
            else numbers = allPages;

            rptPaging.DataSource = numbers;
            rptPaging.DataBind();
        }

        [Description("Aktif sayfa numarası"), Category("Data")]
        public int CurrentPage
        {
            get
            {
                return Convert.ToInt32(this.ViewState["CurrentPage"] ?? 1);
            }
            set
            {
                this.ViewState["CurrentPage"] = value;
            }
        }

        [Description("Sayfa boyutu")]
        public int PageSize
        {
            get
            {
                return Convert.ToInt32(ddlPageSize.SelectedValue);
            }
            set { ddlPageSize.SetSelectedVal(value.ToString()); }
        }

        [Description("Toplam sayfa sayısı"), Category("Data")]
        public int TotalPage
        {
            get
            {
                return Convert.ToInt32(this.ViewState["TotalPage"] ?? 0);
            }
            set
            {
                this.ViewState["TotalPage"] = value;
            }
        }


        [Description("Toplam kayıt sayısı"), Category("Data")]
        public int TotalRecord
        {
            get
            {
                return Convert.ToInt32(this.ViewState["TotalRecord"] ?? 0);
            }
            set
            {
                this.ViewState["TotalRecord"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                this.EnableViewState = true;
            }
        }

        protected void PageNumber_Click(object sender, EventArgs e)
        {
            if (this.PageNumberClick != null)
            {
                LinkButton lb = (sender as LinkButton);
                this.PageNumberClick(sender, new CommandEventArgs(String.IsNullOrEmpty(lb.CommandName) ? "PageNumberClick" : lb.CommandName, lb.CommandArgument));
            }
        }

        protected void PageSize_Changed(object sender, EventArgs e)
        {
            if (this.PageSizeChanged != null)
            {
                this.PageSizeChanged(sender, new CommandEventArgs("PageSizeChanged", Convert.ToInt32(ddlPageSize.SelectedValue)));
            }
        }

        protected int CurrentPageSize
        {
            get
            {
                return Convert.ToInt32(ddlPageSize.SelectedValue);
            }
        }

        protected string GetCurrentViewText
        {
            get
            {
                int startIndex = ((Math.Max(this.CurrentPage, 1) - 1) * this.CurrentPageSize) + 1;
                int endIndex = Math.Min(Math.Max(this.CurrentPage, 1) * this.CurrentPageSize, this.TotalRecord);
                return String.Format("{0}-{1} / {2}", startIndex, endIndex, this.TotalRecord);
            }
        }
    }
}