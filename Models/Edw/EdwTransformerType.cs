using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OlcuYonetimSistemi.Models.Edw
{
    public partial class EdwTransformerType
    {
        public EdwTransformerType()
        {
            IsActive = true;
            LastActivityUserId = OlcuYonetimSistemi.Controllers.CustomMembership.GetActiveUserId();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public Boolean IsActive { get; set; }
        public Guid LastActivityUserId { get; set; }
    }
}