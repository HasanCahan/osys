using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace OlcuYonetimSistemi.Models.Edw
{
    public enum MeasurementType
    {
        [Display(Name = "Belirtilmedi")]
        None = 0,
        Edw = 1,
        Bara = 2,
        Fider = 3
    }
}