using Npgsql;
using System;

namespace PostGIS.RouteNetworkStuff
{
    /// <summary>
    /// Helper function to create a schema containing a route_node and route_segment table
    /// </summary>
    public class RouteNetworkSchemaHelper
    {
        private string _connectionString;
        private string _schemaName = "route_network";
        public RouteNetworkSchemaHelper(string connectionString, string schemaName)
        {
            _connectionString = connectionString;
            _schemaName = schemaName.ToLower();
        }

        /// <summary>
        /// Call this to drop the schema and everything in it
        /// </summary>
        /// <param name="schemaName"></param>
        public void DropSchema()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            // Drop schema if exists
            using (var cmd = new NpgsqlCommand("DROP SCHEMA IF EXISTS " + _schemaName + " CASCADE;", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Creates a schema with route_node and route_segment table in it
        /// </summary>
        /// <param name="schemaName"></param>
        public void CreateSchema()
        {
            // Drop eventually leftovers first
            DropSchema();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            // Make sure postgis extension is installed in database
            using (var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS postgis;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Create schema
            using (var cmd = new NpgsqlCommand("CREATE SCHEMA IF NOT EXISTS " + _schemaName + ";", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Create route node table
            using (var cmd = new NpgsqlCommand(
                "CREATE TABLE " + _schemaName + @".route_node
                (
                    mrid uuid,
                    coord geometry(Point, 25832),
                    work_task_mrid uuid,
                    user_name varchar(255),
                    application_name varchar(255),
                    PRIMARY KEY(mrid)
                );"
                , conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Create spatial index
            using (var cmd = new NpgsqlCommand("CREATE INDEX route_node_coord_idx ON " + _schemaName + ".route_node USING gist(\"coord\"); ", conn))
            {
                cmd.ExecuteNonQuery();
            }


            // Create route segment table
            using (var cmd = new NpgsqlCommand(
                "CREATE TABLE " + _schemaName + @".route_segment
                (
                    mrid uuid,
	                coord geometry(Linestring,25832),
	                work_task_mrid uuid,
	                user_name varchar(255),
	                application_name varchar(255),
	                PRIMARY KEY(mrid)
                );"
                , conn))
            {
                cmd.ExecuteNonQuery();
            }

            // Create spatial index
            using (var cmd = new NpgsqlCommand("CREATE INDEX route_segment_coord_idx ON " + _schemaName + ".route_segment USING gist(\"coord\"); ", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
