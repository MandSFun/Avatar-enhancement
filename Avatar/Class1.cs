using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npgsql;
using System;


namespace PGDemo
{
    class DatabaseSetup
    {
        static void Main(string[] args)
        {
            TestConnection();
            Console.ReadKey();

        }
        private static void TestConnection()
        {
            using (NpgsqlConnection con = GetConnection())
            {
                con.Open();
                if (con.State == System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connected");
                }
            }
        }
        private static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(@"Server=localhost;Port=5432;User ID=postgres;Password=D5ta;Database=AvatarProject;"); ;
        }

    }
}
