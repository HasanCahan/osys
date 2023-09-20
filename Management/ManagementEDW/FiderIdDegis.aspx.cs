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
    public partial class FiderIdDegis : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {  
            btnOkey.Visible = false;
            btnSave.Visible = false;
            ListLog(1);
        }
  
        protected void btnDogrula_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtEskiId.Text) || string.IsNullOrEmpty(txtYeniId.Text)) { return;  }

            if (String.IsNullOrWhiteSpace(txtEskiId.Text.Trim()) || Information.IsFindCharInNumeric(txtEskiId.Text.Trim()))
            {
                lblFiderEskiSonuc.Text = "HATA:numerik değer giriniz";
                return;
            }
            if (String.IsNullOrWhiteSpace(txtYeniId.Text.Trim()) || Information.IsFindCharInNumeric(txtYeniId.Text.Trim()))
            {
                lblFiderYeniSonuc.Text = "HATA:numerik değer giriniz";
                return;
            }


            int eskiFiderId =Convert.ToInt32(txtEskiId.Text);
            int yeniFiderId = Convert.ToInt32(txtYeniId.Text);
            EdwFider getFiderEski = FiderIdDegisModel.GetFider(eskiFiderId);
            EdwFider getFiderYeni = FiderIdDegisModel.GetFider(yeniFiderId);

            btnDogrula1.Attributes.Remove("class"); 

            if (getFiderEski != null && getFiderEski.Id>0)
            {
                lblFiderEskiSonuc.Text = getFiderEski.Name.ToString(); 

            }
            else
            {
                lblFiderEskiSonuc.Text = "Eski Fider ID bulunamadı"; 
            }


            if (getFiderYeni != null && getFiderYeni.Id > 0)
            {
                lblFiderYeniSonuc.Text = getFiderYeni.Name??""; 

            }
            else
            {
                lblFiderYeniSonuc.Text = "Yeni Fider ID bulunamadı"; 
            }


            if (getFiderEski != null && getFiderEski.Id > 0 && getFiderYeni != null && getFiderYeni.Id > 0)
            { 
                btnDogrula1.Attributes.Add("class", "glyphicon glyphicon-ok"); 
                btnSave.Enabled = true;
                btnSave.Visible = true;

            }
            else
            {
                btnDogrula1.Attributes.Add("class", "glyphicon glyphicon-refresh"); 
                btnSave.Enabled = false;
                btnSave.Visible = false;
            }



        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            btnSaveChanges.Visible = false;
            int? count = 0;
            string mesaj = "";
            if (txtYeniId.Text == "" || txtEskiId.Text=="" || txtBeginDate.Text=="" || txtEndDate.Text=="") {
                ucAlert3.Text = "Tüm alanları doldurunuz..";
                modalVisible();
                return; 
            }

            int yeniId = Convert.ToInt32(txtYeniId.Text);
            int eskiId = Convert.ToInt32(txtEskiId.Text);
         
            DateTime baslangicTarih = new DateTime();
            DateTime bitisTarih =new DateTime();
            if (!dateControl(txtBeginDate.Text, ref baslangicTarih) || !dateControl(txtEndDate.Text, ref bitisTarih))
            {
                modalVisible();
                return;

            }
            int? yeniFiderCount = 0;
            int? eskiFiderCount = 0;
            fiderAcmaControl(yeniId, baslangicTarih, bitisTarih, ref  yeniFiderCount);
            fiderAcmaControl(eskiId, baslangicTarih, bitisTarih, ref  eskiFiderCount);


            if (eskiFiderCount<=0)
            { 
                ucAlert3.Text ="Eski Fider ID ye ait kayıt bulunamadı.";
            }
            else
            {
                btnSaveChanges.Visible = true;
                ucAlert3.Text = "Eski Fider ID ye ait açma sayısı:" + eskiFiderCount +
                              "\n Yeni Fider ID ye ait açma sayısı:" + yeniFiderCount + " adet kayıt bulundu";
            }
            

            modalVisible();
        }

        private bool fiderAcmaControl(int fiderId,DateTime baslangicTarih,DateTime bitisTarih, ref int? count)
        {
             count = FiderIdDegisModel.GetFiderAcmaList(fiderId, baslangicTarih, bitisTarih);
            if (count > 0)
            {  
                return true;
            } 

            return false;
            
        }

        public bool dateControl(string gelenDate, ref DateTime date)
        {
            bool isTrue = DateTime.TryParseExact(gelenDate, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out date);
            if(!isTrue)
            {
                ucAlert3.Text = gelenDate + " tarih format dönüştürmede problem oluştu.";
            } 
            return isTrue;
        }

        public void modalVisible()
        {
            upAddEdit.Visible = true;
            Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", String.Format(@"$(function() {{
            $('#{0}').modal('show');
            }});", pnlMessage.ClientID), true);
        }

        protected void txtYeniId_TextChanged(object sender, EventArgs e)
        {
           

            TextBox txt = (TextBox)sender;
            if(txt.Text.Length>2)
            {
                btnSave.Enabled = true;
            }
            else
            {
                btnSave.Enabled = false;
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {

            if (txtYeniId.Text == "" || txtEskiId.Text == "") { return; }

           
            DateTime baslangicTarih = new DateTime();
            DateTime bitisTarih = new DateTime();
            if (!dateControl(txtBeginDate.Text, ref baslangicTarih) || !dateControl(txtEndDate.Text, ref bitisTarih))
            {
                modalVisible();
                return;

            }
              
            FiderIdDegisModel.FiderId fider = new FiderIdDegisModel.FiderId {
                FiderEskiId=Convert.ToInt32(txtEskiId.Text),
                FiderYeniId = Convert.ToInt32(txtYeniId.Text),
                BaslangicTarih = baslangicTarih,
                BitisiTarih = bitisTarih
            };

            Fix(fider);

            DbHelper.DbResponse<bool> result =  FiderIdDegisModel.replaceFiderId(fider);
            if(result.StatusCode == DbHelper.DbResponseStatus.OK)
            {
                btnSaveChanges.Visible = false;
                btnCloseModal.Visible = false;
                btnOkey.Visible = true;
                ucAlert1.Text = "FiderId değişim işlemi başarıyla gerçekleşti.";
            }
            else
            { 
                ucAlert2.Text = "Bir sorun oluştu."+ result.StatusDescription;
            }

             
        }

        private void Fix(FiderIdDegisModel.FiderId FiderId)
        {
            DateTime startDate = FiderId.BaslangicTarih;
            DateTime endDate = FiderId.BitisiTarih;  
            while (startDate<= endDate || (startDate.Year==endDate.Year && startDate.Month==endDate.Month && startDate.Day>=endDate.Day))
            {
                FiderAcma.GenerateNewData(startDate.Year, startDate.Month, FiderId.FiderEskiId);
                FiderAcma.GenerateNewData(startDate.Year, startDate.Month, FiderId.FiderYeniId);
                startDate=startDate.AddMonths(1);
            }
             

        }


        protected void btnOkey_Click(object sender, EventArgs e)
        {
            Response.Redirect("FiderIdDegis.aspx");

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
                ListLog(newIndex);
            }
        }


        protected void PageSize_Changed(object sender, CommandEventArgs e)
        {
            int pageSize = ucPager.PageSize;
            int startIndex = (int)ViewState["StartRowIndex"];
            int newIndex = ((int)Math.Truncate((double)startIndex / (double)pageSize) * pageSize) + 1;
            ListLog(newIndex);


        }
        private void ListLog(Int32 startIndex)
        {
            ViewState["StartRowIndex"] = startIndex; 
            Int32 totalRows = -1; 

           grdPmum.DataSource = FiderIdDegisModel.ListLog(startIndex, ucPager.PageSize, ref totalRows);
                
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

            grdPmum.DataBind();
        }

   


    }
}