using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Linq;

namespace FloridaCounties.Classes {
    public class RootObject {

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

    public class Geometry {
        public float[][][] rings { get; set; }

        public static implicit operator Polygon(Geometry g) {
            LinearRing shell = new LinearRing(g.rings[0].Select(p => new Coordinate(p[0], p[1])).ToArray());
            if (!shell.IsCCW) shell = shell.Reverse() as LinearRing;

            if(g.rings.Length > 1) {
                 LinearRing[] holes = g.rings.Skip(1)
                    .Select(sp => sp.Select(p => new Coordinate(p[0], p[1])))
                    .Select(r => new LinearRing(r.ToArray()))
                    .ToArray();
                
                for(int i = 0; i < holes.Length; i++) {
                    if (holes[i].IsCCW) holes[i] = holes[i].Reverse() as LinearRing;
                }

                Polygon result = new Polygon(shell, holes) { SRID = 4326 };
                return result;
            } else {
                Polygon result = new Polygon(shell) { SRID = 4326};
                return result;
            }
        }
    }
}