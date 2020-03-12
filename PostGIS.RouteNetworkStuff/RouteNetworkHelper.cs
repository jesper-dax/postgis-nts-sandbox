using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace PostGIS.RouteNetworkStuff
{
    /// <summary>
    /// Helps with inserting nodes and segments into the route network using NTS geometry
    /// </summary>
    public class RouteNetworkHelper
    {
        private string _connectionString;
        private string _schemaName = "route_network";

        public RouteNetworkHelper(string connectionString, string schemaName)
        {
            _connectionString = connectionString;
            _schemaName = schemaName.ToLower();
        }

        public Guid InsertNode(Geometry point, Guid? mrid = null, string applicationName = "test", string userName = "test", Guid? workTaskMrid = null)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            // Use mrid provided. If null generate one
            var mridUsed = mrid == null ? Guid.NewGuid() : mrid.Value;

            // Create route node table
            using (var cmd = new NpgsqlCommand("INSERT INTO " + _schemaName + ".route_node " +
                "(mrid, coord, work_task_mrid, user_name, application_name) " +
                "VALUES(@mrid, ST_GeomFromWKB(@coord, 25832), @work_task_mrid, @user_name, @application_name)", 
                conn))
            {
                cmd.Parameters.AddWithValue("mrid", mridUsed);
                cmd.Parameters.AddWithValue("coord", new WKBWriter().Write(point));
                cmd.Parameters.AddWithValue("work_task_mrid", workTaskMrid == null ? Guid.NewGuid() : workTaskMrid.Value);
                cmd.Parameters.AddWithValue("user_name", userName);
                cmd.Parameters.AddWithValue("application_name", applicationName);

                cmd.ExecuteNonQuery();
            }

            return mridUsed;
        }
    }
}
