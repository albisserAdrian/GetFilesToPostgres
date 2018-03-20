using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using Npgsql;

namespace GetFilesToPostgres
{
    class Program
    {
        static void Main(string[] args)
        {
            // Addin folders
            string pathNonUser = @"C:\ProgramData\Autodesk\Revit\Addins\2017";
            string pathUser = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\Revit\Addins\2017");
            // Get Username
            string username = Environment.UserName;
            // Database connection string
            var connString = "Host=;Port=;Username=;Password=;Database=";
            
            Console.WriteLine("Doing something...");

            // Create list from files in directories
            List<string> addinManifests = new List<string>();
            addinManifests.AddRange(Directory.GetFiles(pathUser, "*.addin"));
            addinManifests.AddRange(Directory.GetFiles(pathNonUser, "*.addin"));

            // Delete current user information in DB
            DbDelete(connString, username);

            // Loop over xml files, read the needed information and upload to DB
            XmlRead(connString, addinManifests);

            Console.WriteLine("Done!");
        }

        static void XmlRead(string connString, List<string> addinManifests)
        {
            foreach (string addinManifest in addinManifests)
            {

                try
                {

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(addinManifest);


                    XmlNodeList nodes = xmlDoc.SelectNodes("/RevitAddIns/AddIn");
                    foreach (XmlNode node in nodes)
                    {
                        AddinInfo addinInfo = new AddinInfo();

                        if ((node["Name"]) != null)
                        {
                            addinInfo.Name = node["Name"].InnerText;
                        }
                        else
                        {
                            addinInfo.Name = node["Text"].InnerText;
                        }
                        addinInfo.Manifest = addinManifest;
                        addinInfo.Assembly = node["Assembly"].InnerText;
                        addinInfo.VendorId = node["VendorId"].InnerText;
                        addinInfo.RevitVersion = "2017";
                        addinInfo.AddinEnabled = true;
                        addinInfo.Username = Environment.UserName;
                        addinInfo.CreatedAt = DateTime.UtcNow;

                        DbInsert(connString, addinInfo);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception caught. {e}");
                    continue;
                }
            }
        }

        static void DbInsert(string connString, AddinInfo addinInfo)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO \"AddinsInfo\" (\"Name\", \"Manifest\", \"Assembly\", \"VendorId\", \"RevitVersion\", \"AddinEnabled\", \"Username\", \"CreatedAt\") VALUES (@1,@2,@3,@4,@5,@6,@7,@8)";
                    cmd.Parameters.AddWithValue("1", addinInfo.Name);
                    cmd.Parameters.AddWithValue("2", addinInfo.Manifest);
                    cmd.Parameters.AddWithValue("3", addinInfo.Assembly);
                    cmd.Parameters.AddWithValue("4", addinInfo.VendorId);
                    cmd.Parameters.AddWithValue("5", addinInfo.RevitVersion);
                    cmd.Parameters.AddWithValue("6", addinInfo.AddinEnabled);
                    cmd.Parameters.AddWithValue("7", addinInfo.Username);
                    cmd.Parameters.AddWithValue("8", addinInfo.CreatedAt);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        static void DbDelete(string connString, string username)
        {
            using (var conn = new NpgsqlConnection(connString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM \"AddinsInfo\" WHERE \"Username\"=@username";
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
