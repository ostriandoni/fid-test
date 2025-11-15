using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Fujitsu.Models
{
    [Index(nameof(SupplierCode), IsUnique = true)]
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }
        
        [Required]
        [StringLength(50)]
        [Display(Name = "Supplier Code")]
        public string SupplierCode { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }
        
        [StringLength(50)]
        public string Province { get; set; }
        
        [StringLength(50)]
        public string City { get; set; }
        
        [StringLength(250)]
        public string Address { get; set; }
        
        [StringLength(100)]
        [Display(Name = "PIC")]
        public string ContactPerson { get; set; }
    }
}