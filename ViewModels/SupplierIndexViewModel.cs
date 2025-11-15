using Fujitsu.Models;
using Fujitsu.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Fujitsu.ViewModels
{
    public class SupplierIndexViewModel
    {
        // 1. The list of main data for the page (the grid/table data)
        public IEnumerable<Supplier> Suppliers { get; set; }

        public bool HasResults => Suppliers != null && Suppliers.Any();

        // 2. The selected value from the dropdown filter
        public string SupplierCodeFilter { get; set; }
        public int? SelectedProvinceId { get; set; }
        public int? SelectedCityId { get; set; }

        // 3. The list of options for the dropdown
        public IEnumerable<SelectListItem> ProvinceList { get; set; }
        public IEnumerable<SelectListItem> CityList { get; set; }
    }
}