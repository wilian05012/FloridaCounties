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

            DateTimeConverter dateTimeConverter = new DateTimeConverter();

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

            builder.Entity<FloridaCity>(options => {
                options.ToTable("tblCities")
                    .HasKey(e => e.Id);

                options.Property(entity => entity.Id)
                    .ValueGeneratedNever()
                    .IsRequired();

                options.Property(entity => entity.PlaceFP)
                    .IsRequired();

                options.Property(entity => entity.BebrId)
                    .IsRequired();

                options.Property(entity => entity.Name)
                    .IsRequired();


                options.Property(entity => entity.Notes)
                    .IsRequired(false);

                options.Property(entity => entity.Description)
                    .IsRequired(false);

                options.Property(entity => entity.EntryCreationDate)
                    .HasConversion(dateTimeConverter)
                    .IsRequired();

                options.Property(entity => entity.Area)
                    .IsRequired(true);

                options.Property(entity => entity.Perimeter)
                    .IsRequired(true);

                options.Property(entity => entity.Shape)
                    .IsRequired(true);

                options.Property(entity => entity.CountyId)
                    .IsRequired();

                options.HasOne(options => options.County)
                    .WithMany(county => county.Cities)
                    .HasForeignKey(city => city.CountyId)
                    .HasPrincipalKey(county => county.Id)
                    .OnDelete(DeleteBehavior.Cascade);
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
