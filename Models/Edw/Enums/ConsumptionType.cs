using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace OlcuYonetimSistemi.Models.Edw
{
    public enum ConsumptionType
    {
        [Display(Name="Belirtilmedi")]
        None=0,
        [Display(Name = "Edw Verisi")]
        Edw = 1,
        [Display(Name = "Tki Verisi")]
        Tki = 2,
        [Display(Name = "Fiderlerin Toplamı")]
        Fider = 3,
    }
}