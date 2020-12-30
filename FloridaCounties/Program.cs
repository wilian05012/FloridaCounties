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
using FloridaCounties.Dto.City;

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

        static async Task SaveCountiesToDbAsync(Counties counties, Cities cities) {
            using (FloridaCountiesDbContext dbContext = InitDb()) {
                Console.WriteLine("Cleaning Counties table...");
                await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM tblCounties");
                //Cascade deletion ensures cities are deleted as well

                Console.Write("Saving data to the DB...");
                dbContext.Counties.AddRange(counties.features.Select(feature => new FloridaCounty() {
                    Id = int.Parse(feature.attributes.COUNTY),
                    DepCode = feature.attributes.DEPCODE,
                    EsriId = feature.attributes.OBJECTID,
                    Name = feature.attributes.COUNTYNAME,
                    Shape = feature.geometry
                }));

                dbContext.Cities.AddRange(cities.features.Select(feature => new FloridaCity() { 
                    Id = feature.properties.AUTOID,
                    PlaceFP = int.Parse(feature.properties.PLACEFP),
                    BebrId = feature.properties.BEBR_ID,
                    Name = feature.properties.NAME,
                    Notes = feature.properties.NOTES,
                    Description = feature.properties.DESCRIPT,
                    EntryCreationDate = feature.properties.FGDLAQDATE,
                    Area = feature.properties.SHAPE_AREA,
                    Perimeter = feature.properties.SHAPE_LEN,
                    //Shape = feature.geometry,
                    CountyId = dbContext.Counties.FirstOrDefault(county => county.Name == feature.properties.COUNTY).Id
                }));

                await dbContext.SaveChangesAsync();

                Console.WriteLine(" COMPLETE!");
            }
        }

        //static async Task PopulateFromAPI() {
        //    using (HttpClient client = new HttpClient()) {
        //        Console.WriteLine("Querying the API...");
        //        HttpResponseMessage response = await client.GetAsync(ApiSettings.GetFullqueryUrl());
        //        if (response.IsSuccessStatusCode) {
        //            try {
        //                Console.WriteLine("Parsing response....");
        //                Counties responseContent = await response.Content.ReadAsAsync<Counties>();
        //                await SaveCountiesToDbAsync(responseContent);
        //            } catch(Exception e) {
        //                DisplayError(e);
        //            }
        //        } else {
        //            DisplayError(response.ReasonPhrase);
        //        }
        //    }
        //}

        const string COUNTIES_JSON_DATAFILE = @"DataFiles\Florida_Counties.json";
        static Counties PopulateCountiesFromJsonFile() {
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
            } else {
                Console.WriteLine($"Counties data has been parsed and loaded from {COUNTIES_JSON_DATAFILE}...");
            }

            return fileContent;
            
        }

        const string CITIES_JSON_DATAFILE = @"DataFiles\Florida_Cities.json";
        static Cities PopulateCitiesFromJsonFile() {
            Cities fileContent = null;
            using(StreamReader jsonStreamReader = File.OpenText(CITIES_JSON_DATAFILE)) {
                using(JsonTextReader jsonTextReader = new JsonTextReader(jsonStreamReader)) {
                    jsonTextReader.SupportMultipleContent = false;
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    while(jsonTextReader.Read()) {
                        if(jsonTextReader.TokenType == JsonToken.StartObject) {
                            fileContent = jsonSerializer.Deserialize<Cities>(jsonTextReader);
                        }
                    }
                }
            }

            if(fileContent is null) {
                DisplayError($"Unable to parse json file: {CITIES_JSON_DATAFILE}");
            } else {
                Console.WriteLine($"Cities data has been parsed and loaded from {CITIES_JSON_DATAFILE}");
            }

            return fileContent;
        }

        static async Task Main(string[] args) {
            //await PopulateFromAPI();

            Counties counties = PopulateCountiesFromJsonFile();
            Cities cities = PopulateCitiesFromJsonFile();

            await SaveCountiesToDbAsync(counties, cities);
        }
    }
}
