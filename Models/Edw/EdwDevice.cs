using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public class EdwDevice
    {
        public EdwDevice()
        {
            IsActive = true;
            LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
        }
        public int Id { get; set; }
        public String TransformerCenter { get; set; }
        public String Transformer { get; set; }
        public int? TransformerCenterId { get; set; }
        public int? TransformerId { get; set; }
        public int MeasurementTypeId { get; set; }
        public string MeasurementDescription { get; set; }
        public string Description { get; set; }
        public string DeviceInfo { get; set; }
        public Boolean IsActive { get; set; }
        public Guid LastActivityUserId { get; set; }
    }
}