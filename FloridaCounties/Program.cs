﻿using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using FloridaCounties.Classes;
using FloridaCounties.DataAccess;
using Microsoft.Data.SqlClient;


namespace FloridaCounties {
    public static class ApiSettings {
        public static string BaseUrl => "https://www25.swfwmd.state.fl.us/arcgis12/rest/services/OpenData/Boundaries/MapServer/4/query";
        public static string Query => "where=1%3D1&outFields=*&outSR=4326&f=json";

        public static string GetFullqueryUrl() => $"{BaseUrl}?{Query}";
    }

    public static class DbSettings {
        public static string DatabaseName => "FloridaCounties";
        public static string ServerName => @"(localdb)\mssqllocaldb";
        public static string GetDbCnString() => new SqlConnectionStringBuilder() {
            DataSource = ServerName,
            InitialCatalog = DatabaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        }.ConnectionString;
    }

    public class Program {
        static void DisplayError(string errorMsg) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine($"ERROR: {errorMsg}");
            Console.ResetColor();
        }
        static void DisplayError(Exception e) {
            if(e is AggregateException) {
                foreach(Exception innerException in (e as AggregateException).InnerExceptions) {
                    DisplayError(innerException);
                }
            } else {
                do {
                    if (e.InnerException is null) DisplayError(e.Message);
                    else DisplayError($"{e.Message}{Environment.NewLine}\t{e.InnerException.Message}");

                    e = e.InnerException;
                } while(e != null);
            }
        }

        public static FloridaCountiesDbContext InitDb() => new DesignTimeFloridaDbContextFactory().CreateDbContext(null);

        static async Task Main(string[] args) {
            using (HttpClient client = new HttpClient()) {
                Console.WriteLine("Querying the API...");
                HttpResponseMessage response = await client.GetAsync(ApiSettings.GetFullqueryUrl());
                if (response.IsSuccessStatusCode) {
                    try {
                        Console.WriteLine("Parsing response....");
                        RootObject responseContent = await response.Content.ReadAsAsync<RootObject>();
                        using (FloridaCountiesDbContext dbContext = InitDb()) {
                            Console.WriteLine("Cleaning Counties table...");
                            await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM tblCounties");

                            Console.WriteLine("Saving data to the DB...");
                            dbContext.Counties.AddRange(responseContent.features.Select(feature => new FloridaCounty() {
                                Id = int.Parse(feature.attributes.COUNTY),
                                DepCode = feature.attributes.DEPCODE,
                                EsriId = feature.attributes.OBJECTID,
                                Name = feature.attributes.COUNTYNAME,
                                Polygon = feature.geometry
                            }));

                            await dbContext.SaveChangesAsync();

                            Console.WriteLine("COMPLETE!");
                        }
                    } catch(Exception e) {
                        DisplayError(e);
                    }
                } else {
                    DisplayError(response.ReasonPhrase);
                }
            }
        }
    }
}