using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models
{
    public enum InavitasIdType
    {
        [Display(Name = "Belirtilmedi")]
        None = 0,
        [Display(Name = "Fider")]
        Fider = 1,
        [Display(Name = "Trafo")]
        TrafoId = 2,
        [Display(Name ="Ges")]
        Ges=3
    }
}