using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DapperGraphs.Models
{
    public class EnergyUsageData
    {
        [Key]
        public Guid EnergyUsageDataId { get; set; }

        [Required]
        [Display(Name = "Ano")]
        public short Year { get; set; }
        //Energy use (kg of oil equivalent per capita)
        [Display(Name = "Valor (Kg de óleo equivalente per capita)")]
        public float Value { get; set; }

        public Guid CountryId { get; set; }
        public virtual Country Country { get; set; }
    }
}