using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;
namespace FloridaCounties.Migrations {
    public partial class Programmability : Migration {
        private const string UP_PATTERN = @"\.SqlScripts\.Up\.\w+\.sql$";
        private const string DOWN_PATTERN = @"\.SqlScripts\.Down\.\w+\.sql$";

        private Assembly _executingAssembly = Assembly.GetExecutingAssembly();

        private IEnumerable<string> GetUpSqlScriptNames() {
            Regex regex = new Regex(pattern: UP_PATTERN);
            return _executingAssembly.GetManifestResourceNames().Where(scriptName => regex.IsMatch(scriptName));
        }

        private IEnumerable<string> GetDownSqlScriptNames() {
            Regex regex = new Regex(pattern: DOWN_PATTERN);
            return _executingAssembly.GetManifestResourceNames().Where(scriptName => regex.IsMatch(scriptName));
        }



        protected override void Up(MigrationBuilder migrationBuilder) {
            foreach(string upSqlScriptName in GetUpSqlScriptNames()) {
                using(Stream stream = _executingAssembly.GetManifestResourceStream(upSqlScriptName)) {
                    using(StreamReader streamReader = new StreamReader(stream)) {
                        string sqlCmd = streamReader.ReadToEnd();

                        migrationBuilder.Sql(sqlCmd);
                    }
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder) {
            foreach(string downSqlScrip in GetDownSqlScriptNames()) {
                using(Stream stream = _executingAssembly.GetManifestResourceStream(downSqlScrip)) {
                    using(StreamReader streamReader = new StreamReader(stream)) {
                        string sqlCmd = streamReader.ReadToEnd();

                        migrationBuilder.Sql(sqlCmd);
                    }
                }
            }

        }
    }
}
