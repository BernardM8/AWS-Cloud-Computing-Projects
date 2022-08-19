using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using System.Data;

namespace ProcessS3Event
{
    class Database
    {
        private NpgsqlConnection OpenConnection()
        {
            string endpoint = "mod12pginstance.cykwnx8v2una.us-east-1.rds.amazonaws.com";
            string connString = "Server=" + endpoint + ";" +
                "port=5432;" +
                "Database=SalesDB;" +
                "User ID=postgres;" +
                "password=cs455pass;" +
                "Timeout=15";
            NpgsqlConnection conn = new NpgsqlConnection(connString);
            conn.Open();
            return conn;
        }

        //Method to connect to database and upload queries
        public String uploadToDataBase(VacSiteClass vacSite)
        {
            try
            {
                NpgsqlConnection conn = OpenConnection();
                if (conn.State == ConnectionState.Open)
                {
                    Console.WriteLine("Successfully opened a connection to the database");

                    NpgsqlCommand createTablesCommand = new NpgsqlCommand(
                        vacSite.getInsertTableQuery()+ vacSite.getSiteQuery() + vacSite.getDataQuery(), conn);

                    NpgsqlDataReader lect1 = createTablesCommand.ExecuteReader();                   

                    conn.Close();
                    conn.Dispose();
                }
                else
                {
                    Console.WriteLine("Failed to open a connection to the database. Connection state: {0}",
                        Enum.GetName(typeof(ConnectionState), conn.State));
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine("Npgsql ERROR: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }
            return String.Empty;
        }
    }
}
