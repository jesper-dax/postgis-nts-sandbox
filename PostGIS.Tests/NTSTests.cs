using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PostGIS.Tests
{
    public class NTSTests
    {
        [Fact]
        public void UpdateCoordinates()
        {
            var ls = new LineString(new Coordinate[] { new Coordinate(1, 2), new Coordinate(2, 4) } );

            ls.Coordinates[0] = new Coordinate(2, 3);
        }

       


        [Fact]
        public void CheckIfGeometryIsWithinEnvelope()
        {
            // Extent convering Denmark
            double minX = 380000;
            double maxX = 900000;
            double minY = 6009000;
            double maxY = 6420000;

            // First create excent envelop from params
            var extent = new Envelope(minX, maxX, minY, maxY);

            LineString lineWithinExtent = new WKTReader().Read("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)") as LineString;

            Assert.True(extent.Contains(lineWithinExtent.Envelope.EnvelopeInternal));

            LineString lineOutsideExtent = new WKTReader().Read("LINESTRING(200.64355838 40000.23759438, 578238.4182511 6179279.78494725)") as LineString;

            Assert.False(extent.Contains(lineOutsideExtent.Envelope.EnvelopeInternal));

        }





    }
}
