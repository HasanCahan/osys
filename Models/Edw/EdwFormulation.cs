using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public partial class EdwFormulation
    {
        public EdwFormulation()
        {
            LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
        }
        public int Id { get; set; }
        public short MeasurementTypeId { get; set; }
        public string Sign { get; set; }
        public int StatusHistoryId { get; set; }
        public Guid LastActivityUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public string MeasurementDescription { get; set; }
        public int TransformerCenterId { get; set; }
        public string TransformerCenterName { get; set; }

        public int? StatusTypeId { get; set; }
        public string StatusDescription { get; set; }
        public int? EnergyDirectionTypeId { get; set; }
        public int? DeliveredEnergyId { get; set; }
        public int? ReceivedEnergyId { get; set; }
        public int? PmumId { get; set; }
        public string Comment { get; set; }
        public String EnergyDirectionTypeDescription { get { return EnergyDirectionTypeId.GetDisplayName<EnergyDirectionType>(); } }
    }
}