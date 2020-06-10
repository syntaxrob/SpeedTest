using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedTest
{
    public class ResultsModel
    {
        public string LowestValue { get; set; }
        public string HighestValue { get; set; }
        public string AverageValue { get; set; }
        public string StdDeviation { get; set; }
        public DateTime TimeCompleted { get; set; }
    }
}
