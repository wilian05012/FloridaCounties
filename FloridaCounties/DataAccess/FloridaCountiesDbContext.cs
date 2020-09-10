using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloridaCounties.DataAccess {
    public class FloridaCountiesDbContext: DbContext {
        public DbSet<FloridaCounty> Counties { get; set; }

        public FloridaCountiesDbContext(DbContextOptions<FloridaCountiesDbContext> options) :base(options){ }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<FloridaCounty>(options => {
                options.ToTable("tblCounties")
                    .HasKey(e => e.Id);

                options.Property(entity => entity.Id)
                    .IsRequired(true)
                    .ValueGeneratedNever();

                options.Property(entity => entity.DepCode)
                    .IsRequired(true);

                options.Property(entity => entity.EsriId)
                    .IsRequired(true);

                options.Property(entity => entity.Name)
                    .IsRequired(true)
                    .HasMaxLength(50);

                options.Property(entity => entity.Shape)
                    .IsRequired(true);
            });
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.UseSqlServer(DbSettings.GetDbCnString(), 
                sqlOptions => {
                    sqlOptions.UseNetTopologySuite();
                });
        }
    }
}
