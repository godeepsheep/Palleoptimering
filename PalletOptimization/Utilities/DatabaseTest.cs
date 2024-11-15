using System;
using Microsoft.Data.SqlClient;

namespace PalletOptimization.Utilities
{
    public class DatabaseTest
    {
        private readonly string connectionString = "Server=mssql12.unoeuro.com,1433;Database=straysheep_dk_db_palleoptimering;User Id=straysheep_dk;Password=kpwr269GHdxcBbtmRDze;";

        public void TestConnection()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    Console.WriteLine("Connection to the database was successful!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Connection failed: " + ex.Message);
                }
            }
        }
    }
}