using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public partial class EdwTransformerCenter
    {
        public EdwTransformerCenter()
        {
            IsActive = true;
            LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int? TownId { get; set; }
        public int? CityId { get; set; }
        public Nullable<int> EdwNumber { get; set; }
        public Boolean IsActive { get; set; }
        public string CityName { get; set; }
        public string TownName { get; set; }
        public Guid LastActivityUserId { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

}