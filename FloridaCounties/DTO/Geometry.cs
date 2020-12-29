using System;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Linq;

namespace FloridaCounties.Dto {
    public class Geometry {
        private const int DEFAULT_SRID = 4326;
        public float[][][] rings { get; set; }

        public static implicit operator Polygon(Geometry g) {
            LinearRing shell = new LinearRing(g.rings[0].Select(p => new Coordinate(p[0], p[1])).ToArray());
            if (!shell.IsCCW) shell = shell.Reverse() as LinearRing;

            if (g.rings.Length > 1) {
                LinearRing[] holes = g.rings.Skip(1)
                   .Select(sp => sp.Select(p => new Coordinate(p[0], p[1])))
                   .Select(r => new LinearRing(r.ToArray()))
                   .ToArray();

                for (int i = 0; i < holes.Length; i++) {
                    if (holes[i].IsCCW) holes[i] = holes[i].Reverse() as LinearRing;
                }

                Polygon result = new Polygon(shell, holes) { SRID = DEFAULT_SRID };
                return result;
            } else {
                Polygon result = new Polygon(shell) { SRID = DEFAULT_SRID };
                return result;
            }
        }

        private static Polygon PolygonFromCoordsArray(float[][] ring) {
            LinearRing shell = new LinearRing(ring.Select(point => new Coordinate(point[0], point[1])).ToArray());
            shell = shell.IsCCW ? shell : (LinearRing)shell.Reverse();

            return new Polygon(shell);
        }

        public static implicit operator MultiPolygon(Geometry g) {
            MultiPolygon result = new MultiPolygon(g.rings.Select(ring => PolygonFromCoordsArray(ring)).ToArray()) {
                SRID = DEFAULT_SRID
            };

            return result;
        }
    }
}
