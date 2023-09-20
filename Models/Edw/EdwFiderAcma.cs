using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public class EdwFiderConf
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int FiderId { get; set; }
    }
    public class EdwFiderAcma
    {
        public int Id { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int FiderId { get; set; }
        public int Count { get; set; }
        public String FiderName { get; set; }
        public string Cihaz { get; set; }
        public string TransformerCenterName { get; set; }
        public Guid LastActivityUserId { get; set; }

        public Boolean IsActive { get; set; }
    }
}