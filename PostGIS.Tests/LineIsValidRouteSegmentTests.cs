using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Overlay.Snap;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PostGIS.Tests
{
    public class LineIsValidRouteSegmentTests
    {


        /// <summary>
        /// Logic that checks if a line drawn by user (representing a route segment) is ok to be futher processed
        /// </summary>
        /// <returns></returns>
        private bool LineIsValidRouteSegment(LineString line, double tolerance = 0.01)
        {

            // We don't want lines that has invalid geometry
            if (!line.IsValid)
                return false;

            // We don't want lines that are not simple - i.e. self intersecting
            if (!line.IsSimple)
                return false;

            // We don't want lines that are closes - i.e. where the ends of the line is snapped together
            if (line.IsClosed)
                return false;

            // We don't want ends closer to each other than tolerance
            if (line.StartPoint.Distance(line.EndPoint) < tolerance)
                return false;

            // We don't want ends closer to the edge than tolerance
            if (!GeometrySnapper.SnapToSelf(line, tolerance, false).Equals(line))
                return false;

            return true;
        }



        [Fact]
        public void SimpleLine_Ok()
        {
            //  Simple line, that should give us no problems

            //  O------O
            LineString line = new WKTReader().Read("LINESTRING(578223.64355838 6179284.23759438, 578238.4182511 6179279.78494725)") as LineString;

            Assert.True(LineIsValidRouteSegment(line));
        }

        [Fact]
        public void EndsSnapped_NotOk()
        {
            //  Line with ends snapped together, which is not okay
            //
            //  -------O
            //  |      |
            //  |      |
            //  |------|
                        
            LineString line = new WKTReader().Read("LINESTRING(578241.656539916 6179263.6946997,578230.221332537 6179263.2899136,578229.715349909 6179272.70119047,578241.352950339 6179273.40956615,578241.656539916 6179263.6946997)") as LineString;

            // We should get a true from our line validation logic
            Assert.False(LineIsValidRouteSegment(line));
        }


        [Fact]
        public void SelfInterection_NotOk()
        {
            //  Line self interects, which is not okay
            //
            //         O
            //         |
            //  -----------O
            //  |      |
            //  |      |
            //  |------|

            LineString line = new WKTReader().Read("LINESTRING(578246.766964452 6179292.47246163,578228.753982917 6179292.77605121,578229.867144697 6179305.830403,578241.909531229 6179304.81843774,578239.076028516 6179286.70425968)") as LineString;

            // If the line is self intersection, it is non-simple
            Assert.False(line.IsSimple);

            // We should get a false from our line validation logic
            Assert.False(LineIsValidRouteSegment(line));
        }


        [Fact]
        public void EndSnappedToEdge_NotOk()
        {
            //  Line end snapped to line edge, which is not ok
            //
            //         O
            //         |
            //  -------O
            //  |      |
            //  |      |
            //  |------|

            LineString line = new WKTReader().Read("LINESTRING(578268.32182438 6179249.86872441,578252.029183777 6179249.76752788,578252.332773354 6179266.56615111,578263.261998106 6179266.56615111,578263.667592312 6179249.83981613)") as LineString;

            // This is also a case of self intersection, so IsSimple should be false
            Assert.False(line.IsSimple);

            // We should get a false from our line validation logic
            Assert.False(LineIsValidRouteSegment(line));
        }

        [Fact]
        public void EndsDistanceAtTolerance_Ok()
        {
            //  Line ends 0.01 (tolerance) meters from each other
            //
            //  |-O < 0.01 > O-|
            //  |              |
            //  |              |
            //  |--------------|


            LineString line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578257.898582255 6179230.85514246)") as LineString;

            // We should get a true from our line validation logic
            Assert.True(LineIsValidRouteSegment(line));
        }

        [Fact]
        public void EndsDistanceLessThanTolerance_NotOk()
        {
            //  Line ends are 0.005 (tolerance) meters from each other
            //
            //  |-O < 0.005 > O-|
            //  |               |
            //  |               |
            //  |---------------|


            LineString line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578257.898186956 6179230.84901533)") as LineString;

            // We should get a fase from our line validation logic
            Assert.False(LineIsValidRouteSegment(line));
        }

        [Fact]
        public void EdgeDistanceMoreThanTolerance_Ok()
        {
            //  Line end 0.023 from edge is ok
            //
            //              S
            //              |
            //|--E < 0.023 >|
            //|             |
            //|             |
            //|-------------|

            LineString line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578256.8130914 6179230.85534011)") as LineString;

            // Assert that distance from end point to edge is 0.023
            Coordinate[] linePointsMinusOne = new Coordinate[line.NumPoints - 1];

            for (int i = 0; i < line.NumPoints - 1; i++)
                linePointsMinusOne[i] = line.GetPointN(i).Coordinate;

            LineString newLine = new LineString(linePointsMinusOne);

            var endPointToEdgeDistance = Math.Round(line.EndPoint.Distance(newLine), 3);

            Assert.Equal(0.023, endPointToEdgeDistance);

            // We should get true from our line validation logic
            Assert.True(LineIsValidRouteSegment(line));
        }
        
        [Fact]
        public void EdgeDistanceMoreLessTolerance_NotOk()
        {
            //  Line end 0.023 from edge is not ok
            //
            //              S
            //              |
            //|--E < 0.007 >|
            //|             |
            //|             |
            //|-------------|

            LineString line = new WKTReader().Read("LINESTRING(578257.898582255 6179230.84377762,578248.38610886 6179230.74258109,578248.487305386 6179238.43351703,578257.79738573 6179238.3323205,578256.811707854 6179230.83918227)") as LineString;

            // Assert that distance from end point to edge is 0.007
            Coordinate[] linePointsMinusOne = new Coordinate[line.NumPoints - 1];

            for (int i = 0; i < line.NumPoints - 1; i++)
                linePointsMinusOne[i] = line.GetPointN(i).Coordinate;

            LineString newLine = new LineString(linePointsMinusOne);

            var endPointToEdgeDistance = Math.Round(line.EndPoint.Distance(newLine), 3);

            Assert.Equal(0.007, endPointToEdgeDistance);

            // We should get false from our line validation logic
            Assert.False(LineIsValidRouteSegment(line));
        }

    }
}
