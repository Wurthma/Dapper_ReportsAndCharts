using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DapperGraphs.ViewModels
{
    public class LineGraphJSONViewModel
    {
        public List<string> Countrys { get; set; }
        public List<LineDatumJSONViewModel> ListDataPerYear { get; set; }
    }

    public class LineDatumJSONViewModel
    {
        public short Year { get; set; }
        public IEnumerable<float> ListValues { get; set; }
    }
}