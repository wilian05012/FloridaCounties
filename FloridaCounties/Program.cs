using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using FloridaCounties.Dto.County;
using FloridaCounties.DataAccess;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace FloridaCounties {
    public static class ApiSettings {
        public static string BaseUrl => "https://www25.swfwmd.state.fl.us/arcgis12/rest/services/OpenData/Boundaries/MapServer/4/query";
        public static string Query => "where=1%3D1&outFields=*&outSR=4326&f=json";

        public static string GetFullqueryUrl() => $"{BaseUrl}?{Query}";
    }

    public static class DbSettings {
        public static string DatabaseName => "GeoFlorida";
        public static string ServerName => @"wa186063";
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

        static async Task SaveRootObjectToDbAsync(Counties rootObject) {
            using (FloridaCountiesDbContext dbContext = InitDb()) {
                Console.WriteLine("Cleaning Counties table...");
                await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM tblCounties");

                Console.WriteLine("Saving data to the DB...");
                dbContext.Counties.AddRange(rootObject.features.Select(feature => new FloridaCounty() {
                    Id = int.Parse(feature.attributes.COUNTY),
                    DepCode = feature.attributes.DEPCODE,
                    EsriId = feature.attributes.OBJECTID,
                    Name = feature.attributes.COUNTYNAME,
                    Shape = feature.geometry
                }));

                await dbContext.SaveChangesAsync();

                Console.WriteLine("COMPLETE!");
            }
        }

        static async Task PopulateFromAPI() {
            using (HttpClient client = new HttpClient()) {
                Console.WriteLine("Querying the API...");
                HttpResponseMessage response = await client.GetAsync(ApiSettings.GetFullqueryUrl());
                if (response.IsSuccessStatusCode) {
                    try {
                        Console.WriteLine("Parsing response....");
                        Counties responseContent = await response.Content.ReadAsAsync<Counties>();
                        await SaveRootObjectToDbAsync(responseContent);
                    } catch(Exception e) {
                        DisplayError(e);
                    }
                } else {
                    DisplayError(response.ReasonPhrase);
                }
            }
        }

        const string COUNTIES_JSON_DATAFILE = @"DataFiles\Florida_Counties.json";
        static async Task PopulateFromJsonFile() {
            Counties fileContent = null;
            using(StreamReader jsonStreamReader = File.OpenText(COUNTIES_JSON_DATAFILE)) {
                using (JsonTextReader jsonTextReader = new JsonTextReader(jsonStreamReader)) {
                    jsonTextReader.SupportMultipleContent = false;
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    while(jsonTextReader.Read()) {
                        if(jsonTextReader.TokenType == JsonToken.StartObject) {
                            fileContent = jsonSerializer.Deserialize<Counties>(jsonTextReader);
                        }
                    }
                }
            }
            
            if(fileContent is null) {
                DisplayError($"Unable to parse json file: {COUNTIES_JSON_DATAFILE}");
                return;
            }

            Console.WriteLine($"Data has been parsed and loaded from {COUNTIES_JSON_DATAFILE}...");

            await SaveRootObjectToDbAsync(fileContent);
        }

        static async Task Main(string[] args) {
            //await PopulateFromAPI();

            await PopulateFromJsonFile();
        }
    }
}
