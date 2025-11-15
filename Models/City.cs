using System.ComponentModel.DataAnnotations.Schema;

namespace Fujitsu.Models
{
    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; }
        
        public int ProvinceId { get; set; } 
        
        [ForeignKey("ProvinceId")]
        public Province Province { get; set; }
    }
}