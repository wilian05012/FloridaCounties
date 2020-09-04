using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using Xunit;
using SUT = FloridaCounties;
 

namespace FloridaCountiesTest {
    public class ConversionsTests {
        private float[][][] simpleRing = {
            new float[][] {
                new float[] { 1, 1 },
                new float[] { 3, 3 },
                new float[] { 3, 1 },
                new float[] { 1, 1 }
            }
        };

        private float[][][] multipleRings = {
            new float[][] {
                new float[] { 1, 1 },
                new float[] { 5, 1 },
                new float[] { 5, 5 },
                new float[] { 1, 5 },
                new float[] { 1, 1 }
            },
            new float[][] {
                new float[] { 2, 2 },
                new float[] { 4, 2 },
                new float[] { 4, 4 },
                new float[] { 2, 4 },
                new float[] { 2, 2 }
            }
        };
        

        [Fact]
        public void CanConvertToPolygon() {
            //Arranging
            SUT.Classes.Geometry simpleGeometry = new SUT.Classes.Geometry() { rings = simpleRing };
            SUT.Classes.Geometry multiGeometry = new SUT.Classes.Geometry() { rings = multipleRings };

            //Acting
            Polygon simplePolygon = simpleGeometry;
            Polygon multiplePolygon = multiGeometry;

            //Assert
            Assert.True(simplePolygon.IsValid);
            Assert.True(simplePolygon.IsSimple);

            Assert.True(multiplePolygon.IsValid);
            Assert.True(multiplePolygon.IsSimple);
        }
    }
}
