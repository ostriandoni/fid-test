using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fujitsu.ViewModels
{
    public class ProvinceViewModel
    {
        [Required]
        [Display(Name = "Province")]
        public int SelectedProvinceId { get; set; }

        public IEnumerable<SelectListItem> ProvinceList { get; set; }
        
        public string Name { get; set; }
    }
}