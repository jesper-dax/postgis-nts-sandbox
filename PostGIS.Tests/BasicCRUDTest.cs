using Microsoft.Extensions.Configuration;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using PostGIS.RouteNetworkStuff;
using System;
using Xunit;

namespace PostGIS.Tests
{
    public class BasicCRUDTests : IDisposable
    {
        private IConfigurationRoot _config;
        private string _schemaName = null;

        public BasicCRUDTests()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("test_config.json")
                .Build();
        }

        public void Dispose()
        {
            /*
            -- udkommenteret fordi det var rar lige at kunne kigge i resultatet fra QGIS og pgAdmin
            if (_schemaName != null)
            {
                new RouteNetworkSchemaHelper(_config["ConnectionString"], _schemaName)
                    .DropSchema();
            }
            */
        }

        private void CreateSchema(string schemaName)
        {
            _schemaName = schemaName;

            new RouteNetworkSchemaHelper(_config["ConnectionString"], _schemaName)
                    .CreateSchema();
        }

        [Fact]
        public void SimpleGeometryInsertQueryTest()
        {
            CreateSchema(System.Reflection.MethodBase.GetCurrentMethod().Name);

            var routeNetwork = new RouteNetworkHelper(_config["ConnectionString"], _schemaName);

            // Insert a node somewhere on the Endelave island
            var pntToInsert = new Point(579886, 6179972);
            routeNetwork.InsertNode(pntToInsert);

            // Try query it
            using var conn = new NpgsqlConnection(_config["ConnectionString"]);
            conn.Open();

            using (var cmd = new NpgsqlCommand("SELECT ST_AsBinary(coord) from " + _schemaName + ".route_node", conn))
            using (var reader = cmd.ExecuteReader())
            {
                // read first row
                reader.Read();

                var pntRead = new WKBReader().Read((byte[])reader[0]);

                Assert.Equal(pntToInsert, pntRead);
            }

        }
    }
}
