using Fujitsu.Models;
using Fujitsu.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Fujitsu.ViewModels
{
    public class UpdateSupplierModel
    {
        // CRITICAL: ID is required for updating
        public int SupplierId { get; set; }

        // This is sent but NOT used for lookup/update since it's disabled/unique
        public string SupplierCode { get; set; }

        public string SupplierName { get; set; }
        public int? ProvinceId { get; set; }
        public int? CityId { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
    }
}