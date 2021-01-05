using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloridaCounties.DataAccess {
    public class FloridaCity {
        public int Id { get; set; }
        public int PlaceFP { get; set; }
        public int BebrId { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public string Description { get; set; }
        public DateTime EntryCreationDate { get; set; }
        public double Area { get; set; }
        public double Perimeter { get; set; }

        public Geometry Shape { get; set; }

        public int CountyId { get; set; }
        public FloridaCounty County { get; set; }
    }
}
