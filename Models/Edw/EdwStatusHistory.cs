using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public partial class EdwStatusHistory
    {
        public EdwStatusHistory()
        {
            LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
            CreationTime = DateTime.Now;
            IsActive = true;
        }
        public int Id { get; set; }
        public System.DateTime CreationTime { get; set; }
        public DateTime? EndDate { get; set; }
        public Boolean HasEnded { get { return EndDate != null; } }
        public int StatusId { get; set; }
        public int? CityId { get; set; }
        public int? TownId { get; set; }
        public int? PmumId { get; set; }
        public Nullable<Int32> TransformerCenterEdwNumber { get; set; }
        //public Nullable<int> TransformerTypeId { get; set; }
        //public string TransformerTypeDescription { get; set; }
        public Nullable<int> ReceivedEnergyId { get; set; }
        public Nullable<int> DeliveredEnergyId { get; set; }
        public string TransformerCenterName { get; set; }
        public string TransformerName { get; set; }
        public string StatusDescription { get; set; }
        public int TransformerId { get; set; }
        public Nullable<int> TransformerCenterId { get; set; }
        public Guid LastActivityUserId { get; set; }
        public Nullable<int> EdwConsumptionTypeId { get; set; }
        public Nullable<int> BaraConsumptionTypeId { get; set; }
        public Nullable<int> FiderConsumptionTypeId { get; set; }
        public Nullable<int> OsosNumber { get; set; }
        public string EdwConsumptionTypeDescription { get { return EdwConsumptionTypeId.GetDisplayName<ConsumptionType>(); } }
        public string BaraConsumptionTypeDescription { get { return BaraConsumptionTypeId.GetDisplayName<ConsumptionType>(); } }
        public string FiderConsumptionTypeDescription { get { return FiderConsumptionTypeId.GetDisplayName<ConsumptionType>(); } }
        public string Comment { get; set; }
        public Boolean IsActive { get; set; }
    }
}