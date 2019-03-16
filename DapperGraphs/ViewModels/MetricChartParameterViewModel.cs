using DapperGraphs.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DapperGraphs.ViewModels
{
    public class MetricChartParameterViewModel
    {
        [Required]
        [Display(Name = "Ano inicial")]
        public short AnoInicial { get; set; }

        [Required]
        [Display(Name = "Ano final")]
        public short AnoFinal { get; set; }

        public IEnumerable<string> ListaIdPaises { get; set; }

        public IEnumerable<SelectListItem> ListaPaises { get; set; }
    }
}