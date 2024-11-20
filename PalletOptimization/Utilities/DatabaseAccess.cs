﻿using System;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;

namespace PalletOptimization.Utilities
{
    public sealed class DatabaseAccess
    {
        private static readonly string connectionString = "Server=mssql12.unoeuro.com,1433;Database=straysheep_dk_db_palleoptimering;User Id=straysheep_dk;Password=kpwr269GHdxcBbtmRDze;";
        private static DatabaseAccess? instance;
        SqlConnection connection;
        public DatabaseAccess()
        {
            OpenConnection();
        }

        public static DatabaseAccess GetInstance()
        {
            if (instance == null)
            {
                instance = new DatabaseAccess();
            }
            return instance;
        }

        public void OpenConnection()
        {
            connection = new SqlConnection(connectionString);
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

        public void ExecuteQuery(string sqlQuery)
        {
            SqlCommand command = new SqlCommand(sqlQuery, connection);
            try
            {
                SqlDataReader reader = command.ExecuteReader();
                Console.WriteLine("Command Executed Successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Command Failed, Message: \"{e.Message}\"");
            }

        }

    }
}