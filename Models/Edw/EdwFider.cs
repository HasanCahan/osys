using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public partial class EdwFider
    {
        public Boolean changed;
        public EdwFider()
        {
            IsActive = true;
            LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int TransformerCenterId { get; set; }
        public string TransformerCenterName { get; set; }
        public Boolean IsActive { get; set; }
        public Guid LastActivityUserId { get; set; }
        public string Comment { get; set; }
        public override string ToString()
        {
            return $"Trafo Merkezi: {TransformerCenterName} , Ad: {Name}";
        }
    }
}