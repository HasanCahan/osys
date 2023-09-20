using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public partial class EdwTransformer
    {
        public Boolean changed;
        public EdwTransformer()
        {
            IsActive = true;
            LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int TransformerCenterId { get; set; }
        public Int32 PmumNumber { get; set; }
        //public int? TransformerTypeId { get; set; }
        public int? InavitasIdType { get; set; }
        public int? ReceivedEnergyId { get; set; }
        public int? DeliveredEnergyId { get; set; }
        public string TransformerCenterName { get; set; }
        //public string TransformerType { get; set; }
        public Boolean IsActive { get; set; }
        public Guid LastActivityUserId { get; set; }
        public int? OsosNumber { get; set; }
        public string Comment { get; set; }
        public override string ToString()
        {
            return $"Edw: {PmumNumber} , InavitasId:{OsosNumber}, Ad: {Name}";
        }
    }
}