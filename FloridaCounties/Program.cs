using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using FloridaCounties.Dto.County;
using FloridaCounties.DataAccess;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using FloridaCounties.Dto.City;
using NetTopologySuite.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;

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

        static async Task SaveCountiesToDbAsync(Counties counties) {
            using (FloridaCountiesDbContext dbContext = InitDb()) {
                Console.WriteLine("Cleaning Counties table...");
                await dbContext.Database.ExecuteSqlRawAsync(@"DELETE FROM tblCounties");
                //Cascade deletion ensures cities are deleted as well

                Console.Write("Saving data to the DB...");
                dbContext.Counties.AddRange(counties.features.Select(feature => new FloridaCounty() {
                    Id = int.Parse(feature.attributes.COUNTY),
                    DepCode = feature.attributes.DEPCODE,
                    EsriId = feature.attributes.OBJECTID,
                    Name = feature.attributes.COUNTYNAME != "DADE" ? feature.attributes.COUNTYNAME : "MIAMI-DADE",
                    Shape = feature.geometry
                }));
                await dbContext.SaveChangesAsync();
                Console.WriteLine("COMPLETE.");

                Console.Write("Parsing cities....");
                IEnumerable<FloridaCity> cities = PopulateCitiesFromJsonFile(dbContext);
                dbContext.AddRange(cities);
                Console.WriteLine("COMPLETE.");

                Console.Write("Saving cities to DB....");
                await dbContext.SaveChangesAsync();
                Console.WriteLine("COMPLETE.");

                Console.WriteLine(" DONE!");
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
        static IEnumerable<FloridaCity> PopulateCitiesFromJsonFile(FloridaCountiesDbContext dbContext) {
            FeatureCollection fileContent = null;
            var jsonSerializer = GeoJsonSerializer.Create();
            using(StreamReader jsonStreamReader = File.OpenText(CITIES_JSON_DATAFILE)) {
                using(JsonTextReader jsonTextReader = new JsonTextReader(jsonStreamReader)) {
                    jsonTextReader.SupportMultipleContent = false;
                    while(jsonTextReader.Read()) {
                        if(jsonTextReader.TokenType == JsonToken.StartObject) {
                            fileContent = jsonSerializer.Deserialize<FeatureCollection>(jsonTextReader);
                        } 
                    }
                }
            }

            if (fileContent != null) {
                return fileContent.Select(feature => {
                    FloridaCity floridaCity = new FloridaCity() {
                        Id = Convert.ToInt32(feature.Attributes["AUTOID"]),
                        PlaceFP = Convert.ToInt32(feature.Attributes["PLACEFP"]),
                        BebrId = Convert.ToInt32(feature.Attributes["BEBR_ID"]),
                        Name = feature.Attributes["NAME"].ToString(),
                        Notes = feature.Attributes["NOTES"]?.ToString(),
                        Description = feature.Attributes["DESCRIPT"]?.ToString(),
                        EntryCreationDate = DateTime.Parse(feature.Attributes["FGDLAQDATE"].ToString()),
                        Area = (double)feature.Attributes["SHAPE_AREA"],
                        Perimeter = (double)feature.Attributes["SHAPE_LEN"]
                    };

                    try {
                        string countyName = feature.Attributes["COUNTY"].ToString().Split(',')[0];
                        countyName = countyName == "ST LUCIE" ? "ST. LUCIE" : countyName;
                        countyName = countyName == "ST JOHNS" ? "ST. JOHNS" : countyName;
                        floridaCity.CountyId = dbContext.Counties.First(county => county.Name == countyName).Id;
                    } catch {
                        throw;
                    }

                    switch(feature.Geometry.GeometryType) {
                        case "Polygon":
                            Polygon polygon = feature.Geometry as Polygon;
                            polygon = polygon.Shell.IsCCW ? polygon :
                                new Polygon(polygon.Shell.Reverse() as LinearRing, polygon.Holes) { SRID = 4326 };

                            for(int i =0; i < polygon.Holes.Length; i++) {
                                LinearRing hole = polygon.Holes[i];
                                hole = hole.IsCCW ? hole.Reverse() as LinearRing : hole;

                                polygon.Holes[i] = hole;
                            }

                            floridaCity.Shape = polygon;
                            break;
                        case "MultiPolygon":
                            MultiPolygon multiPolygon = feature.Geometry as MultiPolygon;
                            for(int i = 0; i < multiPolygon.Geometries.Length; i++) {
                                Polygon p = multiPolygon.Geometries[i] as Polygon;

                                p = p.Shell.IsCCW ? p :
                                    new Polygon(p.Shell.Reverse() as LinearRing, p.Holes) { SRID = 4326 };

                                for(int j = 0; j < p.Holes.Length; j++) {
                                    LinearRing hole = p.Holes[j];
                                    hole = hole.IsCCW ? hole.Reverse() as LinearRing : hole;

                                    p.Holes[j] = hole;
                                }

                                multiPolygon.Geometries[i] = p;
                            }


                            floridaCity.Shape = multiPolygon;
                            break;
                    }

                    return floridaCity;
                });
            } else return null;
        }

        static async Task Main(string[] args) {
            //await PopulateFromAPI();

            
            Counties counties = PopulateCountiesFromJsonFile();
            await SaveCountiesToDbAsync(counties);
        }
    }
}
