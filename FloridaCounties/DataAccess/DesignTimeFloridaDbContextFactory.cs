using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloridaCounties.DataAccess {
    public class DesignTimeFloridaDbContextFactory : IDesignTimeDbContextFactory<FloridaCountiesDbContext> {
        public FloridaCountiesDbContext CreateDbContext(string[] args) {
            DbContextOptionsBuilder<FloridaCountiesDbContext> builder = new DbContextOptionsBuilder<FloridaCountiesDbContext>();
            builder.UseSqlServer(DbSettings.GetDbCnString(), 
                sqlOptions => {
                    sqlOptions.UseNetTopologySuite();
                });
            

            return new FloridaCountiesDbContext(builder.Options);
        }
    }
}
