using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace OlcuYonetimSistemi.Models.Edw
{
    public enum EnergyDirectionType
    {
        [Display(Name = "Belirtilmedi")]
        None=0,
        [Display(Name = "Alınan")]
        Received = 1
        ,
        [Display(Name = "Verilen")]
        Delivered = 2
    }
}