using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DapperGraphs.ViewModels
{
    public class LineGraphDataViewModel
    {
        public Guid CountryId { get; set; }
        public string Name { get; set; }
        public short Year { get; set; }
        public float Value { get; set; }
    }
}