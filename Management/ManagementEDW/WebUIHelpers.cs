using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
namespace OlcuYonetimSistemi.Management.ManagementEDW
{
    public static class WebUIHelpers
    {
        public static void SetSelectedVal(this DropDownList ddL, Object val)
        {
            try
            {
                if (val == null)
                    ddL.ClearSelection();
                ddL.SelectedValue = val == null ? null : val.ToString();
            }
            catch
            {

            }
        }
    }
}