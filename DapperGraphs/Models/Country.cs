using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DapperGraphs.Models
{
    public class Country
    {
        [Key]
        public Guid CountryId { get; set; }

        [Required]
        [StringLength(200)]
        [Index("IUQ_Countries_Name", IsUnique = true)]
        public string Name { get; set; }

        public virtual ICollection<EnergyUsageData> EnergyUsageData { get; set; }
    }
}