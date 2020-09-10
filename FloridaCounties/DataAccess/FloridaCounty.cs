using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FloridaCounties.DataAccess {
    public class FloridaCounty {
        public int Id { get; set; }
        public int DepCode { get; set; }
        public int EsriId { get; set; }
        public string Name { get; set; }
        public MultiPolygon Shape { get; set; }
    }
}
