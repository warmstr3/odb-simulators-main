using System;
using CsvHelper.Configuration.Attributes;

namespace Simulator.Dnp3.Configurator
{
    public class Dnp3Point
    {
        [Name("Point Name")]
        public string PointName { get; set; }
        [Name("Volttron Point Name")]
        public string VolttronPointName { get; set; }
        public int Group { get; set; }
        public int Variation { get; set; }
        public int Index { get; set; }
        public double Scaling { get; set; }
        public string Units { get; set; }
        public bool Writable { get; set; }
        public string Notes { get; set; }

        public static Dnp3Point FromCsv(string csvLine)
        {
            var values = csvLine.Split(',');
            return new Dnp3Point
            {
                PointName = values[0],
                VolttronPointName = values[1],
                Group = int.Parse(values[2]),
                Variation = int.Parse(values[3]),
                Index = int.Parse(values[4]),
                Scaling = double.Parse(values[5]),
                Units = values[6],
                Writable = values[7].Trim().ToUpper() == "TRUE",
                Notes = values[8]
            };
        }
    }
}