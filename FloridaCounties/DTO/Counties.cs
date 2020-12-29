using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Linq;

namespace FloridaCounties.Dto.County {
    public class Counties {

        public Feature[] features { get; set; }
    }

    public class Feature {
        public Attributes attributes { get; set; }
        public Geometry geometry { get; set; }
    }

    public class Attributes {
        public int OBJECTID { get; set; }
        public string COUNTYNAME { get; set; }
        public int DEPCODE { get; set; }
        public string COUNTY { get; set; }
    }
}