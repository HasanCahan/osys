using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace OlcuYonetimSistemi.Management
{
    public partial class CalcParamAddEdit : System.Web.UI.Page
    {
        protected int CalcParamId
        {
            get
            {
                return (int)(ViewState["CalcParamId"] ?? 0);
            }
            set
            {
                ViewState["CalcParamId"] = value;
            }
        }
        protected int EquipmentId
        {
            get
            {
                return (int)(ViewState["EquipmentId"] ?? 0);
            }
            set
            {
                ViewState["EquipmentId"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                for (int y = 2015; y <= DateTime.Today.Year; y++) { ddlDataPeriod.Items.Add(y.ToString()); }

                ddlDataPeriod.Attributes["onchange"] = String.Format("if(ChangeConfirm(this, event)) {0};", Page.ClientScript.GetPostBackEventReference(ddlDataPeriod, String.Empty).Trim());
                ddlReadSource.Attributes["onchange"] = String.Format("if(ChangeConfirm(this, event)) {0};", Page.ClientScript.GetPostBackEventReference(ddlReadSource, String.Empty).Trim());

                Models.tCalcParam param = null;
                if (Request["equipmentid"] != null && Information.IsNumeric(Request["equipmentid"].ToString().Trim()))
                {
                    LoadData(Convert.ToInt32(Request["equipmentid"].ToString().Trim()), DateTime.Today.Year, null, out param);
                }
                if (param == null)
                {
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                    ucAlert.Text = "İstenilen ekipman bulunamadı.";
                    btnSaveChanges.Visible = false;
                    btnDelete.Visible = false;
                }
                else
                {
                    this.EquipmentId = param.EquipmentId;
                    this.CalcParamId = param.CalcParamId;
                    btnDelete.Visible = this.CalcParamId > 0;
                }
            }
            else
            {
                ucAlert.Text = null;
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (EquipmentId < 1)
            {
                ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Warning;
                ucAlert.Text = "Ekipman belirlenemediği için işlem iptal edildi.";
                txtEquipmentName.Focus();
                return;
            };

            if (ddlReadSource.SelectedIndex < 1)
            {
                ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Warning;
                ucAlert.Text = "Veri kaynağını seçiniz.";
                ddlReadSource.Focus();
                return;
            }

            if (String.IsNullOrWhiteSpace(txtkVA.Text) || Helper.ConvertDouble(txtkVA.Text).GetValueOrDefault(0) <= 0)
            {
                ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Warning;
                ucAlert.Text = "Geçerli bir ekipman gücü giriniz.";
                txtkVA.Focus();
                return;
            }

            TextBox[] txtK = new TextBox[] { txtK1, txtK2, txtK3, txtK4, txtK5, txtK6, txtK7, txtK8, txtK9, txtK10, txtK11, txtK12 };
            foreach (TextBox myK in txtK)
            {
                if (!String.IsNullOrWhiteSpace(myK.Text) && !Helper.ConvertDouble(myK.Text).GetValueOrDefault(-1).Between(0, 1))
                {
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Warning;
                    ucAlert.Text = "Geçersiz katsayı.";
                    myK.Focus();
                    return;
                }
            }

            Models.tCalcParam param = new Models.tCalcParam()
            {
                CalcParamId = this.CalcParamId,
                EquipmentId = this.EquipmentId,
                ReadSource = ddlReadSource.SelectedValue,
                kVA = Helper.ConvertDouble(txtkVA.Text),
                K1 = Helper.ConvertDouble(txtK1.Text),
                K2 = Helper.ConvertDouble(txtK2.Text),
                K3 = Helper.ConvertDouble(txtK3.Text),
                K4 = Helper.ConvertDouble(txtK4.Text),
                K5 = Helper.ConvertDouble(txtK5.Text),
                K6 = Helper.ConvertDouble(txtK6.Text),
                K7 = Helper.ConvertDouble(txtK7.Text),
                K8 = Helper.ConvertDouble(txtK8.Text),
                K9 = Helper.ConvertDouble(txtK9.Text),
                K10 = Helper.ConvertDouble(txtK10.Text),
                K11 = Helper.ConvertDouble(txtK11.Text),
                K12 = Helper.ConvertDouble(txtK12.Text),
                DataPeriod = Convert.ToInt32(ddlDataPeriod.SelectedValue)
            };

            DbHelper.DbResponse<Models.tCalcParam> saveResp = Controllers.CalcParam.SaveCalcParam(param);
            switch (saveResp.StatusCode)
            {
                case DbHelper.DbResponseStatus.OK:
                    Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", "dismissModal();", true);
                    break;
                default:
                    ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                    ucAlert.Text = String.IsNullOrEmpty(saveResp.StatusDescription) ? "Kayıt yapılamıyor, lütfen yeniden deneyiniz." : saveResp.StatusDescription;
                    break;
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (this.CalcParamId > 0)
            {
                DbHelper.DbResponse resp = Controllers.CalcParam.DeleteCalcParam(this.CalcParamId);
                switch (resp.StatusCode)
                {
                    case DbHelper.DbResponseStatus.NotFound:
                    case DbHelper.DbResponseStatus.OK:
                        Page.ClientScript.RegisterStartupScript(Page.GetType(), "jsAddEdit", "dismissModal();", true);
                        break;
                    default:
                        ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                        ucAlert.Text = resp.StatusDescription;
                        break;
                }
            }
        }

        private void LoadData(Int32 equipmentId, Nullable<int> dataPeriod, string readSource, out Models.tCalcParam calcParam)
        {
            calcParam = Controllers.CalcParam.GetCalcParamByEquipment(equipmentId, dataPeriod, readSource);
            if (calcParam != null)
            {
                txtEquipmentName.Text = calcParam.EquipmentName;
                if (calcParam.DataPeriod.HasValue && calcParam.DataPeriod > 0)
                {
                    ddlDataPeriod.ClearSelection();
                    ddlDataPeriod.SelectedValue = calcParam.DataPeriod.ToString();
                }
                if (!String.IsNullOrWhiteSpace(calcParam.ReadSource))
                {
                    ddlReadSource.ClearSelection();
                    ddlReadSource.SelectedValue = calcParam.ReadSource;
                }
                txtkVA.Text = calcParam.kVA.HasValue ? calcParam.kVA.ToString() : String.Empty;
                txtK1.Text = calcParam.K1.HasValue ? calcParam.K1.ToString() : String.Empty;
                txtK2.Text = calcParam.K2.HasValue ? calcParam.K2.ToString() : String.Empty;
                txtK3.Text = calcParam.K3.HasValue ? calcParam.K3.ToString() : String.Empty;
                txtK4.Text = calcParam.K4.HasValue ? calcParam.K4.ToString() : String.Empty;
                txtK5.Text = calcParam.K5.HasValue ? calcParam.K5.ToString() : String.Empty;
                txtK6.Text = calcParam.K6.HasValue ? calcParam.K6.ToString() : String.Empty;
                txtK7.Text = calcParam.K7.HasValue ? calcParam.K7.ToString() : String.Empty;
                txtK8.Text = calcParam.K8.HasValue ? calcParam.K8.ToString() : String.Empty;
                txtK9.Text = calcParam.K9.HasValue ? calcParam.K9.ToString() : String.Empty;
                txtK10.Text = calcParam.K10.HasValue ? calcParam.K10.ToString() : String.Empty;
                txtK11.Text = calcParam.K11.HasValue ? calcParam.K11.ToString() : String.Empty;
                txtK12.Text = calcParam.K12.HasValue ? calcParam.K12.ToString() : String.Empty;
            }
        }

        protected void Datakey_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.EquipmentId < 1)
            {
                ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                ucAlert.Text = "İstenilen ekipman bulunamadı.";
                btnSaveChanges.Visible = false;
                btnDelete.Visible = false;
                return;
            }

            if (ddlReadSource.SelectedIndex == 0)
            {
                ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Warning;
                ucAlert.Text = "Veri kaynağını seçiniz.";
                btnSaveChanges.Visible = false;
                btnDelete.Visible = false;
                return;
            }

            Models.tCalcParam param = null;
            LoadData(this.EquipmentId, Convert.ToInt32(ddlDataPeriod.SelectedValue), ddlReadSource.SelectedValue, out param);
            if (param == null)
            {
                ucAlert.AlertType = OlcuYonetimSistemi.Management.ucAlert.AlertTypes.Danger;
                ucAlert.Text = "İstenilen ekipman bulunamadı.";
                btnSaveChanges.Visible = false;
                btnDelete.Visible = false;
            }
            else
            {
                this.CalcParamId = param.CalcParamId;
                btnSaveChanges.Visible = true;
                btnDelete.Visible = this.CalcParamId > 0;
            }

        }
    }
}