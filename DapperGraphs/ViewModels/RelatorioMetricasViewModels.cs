using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DapperGraphs.ViewModels
{
    public class RelatorioMetricasViewModels
    {
        [Display(Name = "País")]
        public string NomePais { get; set; }

        [Display(Name = "Média")]
        public float Media { get; set; }

        [Display(Name = "Desvio Padrão")]
        public float DesvioPadrao { get; set; }

        [Display(Name = "Mínimo")]
        public float Minimo { get; set; }

        [Display(Name = "Máximo")]
        public float Maximo { get; set; }
    }
}