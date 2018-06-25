using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Annotations;
using DigitalHealth.StoreAndForward.Core.Data.Entities;
using Serilog;
using Serilog.Events;

namespace DigitalHealth.StoreAndForward.Infrastructure.Data
{
    public class StoreAndForwardDbContext : DbContext
    {
        public virtual DbSet<DocumentEntity> Documents { get; set; }
        public virtual DbSet<EventEntity> Events { get; set; }

        public StoreAndForwardDbContext() : base("StoreAndForwardDb")
        {
            Log.Information("Creating 'StoreAndForwardDbContext'");

            Database.Log = s =>
            {
                if (Log.IsEnabled(LogEventLevel.Verbose))
                {
                    Log.Information("{entityFrameworkLog}", s);
                }                
            };
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));

            // Document
            modelBuilder.Entity<DocumentEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<DocumentEntity>().Property(e => e.DocumentId).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<DocumentEntity>().Property(e => e.DocumentIdToReplace).HasMaxLength(50);
            modelBuilder.Entity<DocumentEntity>().Property(e => e.DocumentData).IsRequired();
            modelBuilder.Entity<DocumentEntity>().Property(e => e.FormatCode).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<DocumentEntity>().Property(e => e.FormatCodeName).IsRequired();
            modelBuilder.Entity<DocumentEntity>().Property(e => e.Status).IsRequired();
            modelBuilder.Entity<DocumentEntity>().Property(e => e.Ihi).IsRequired().HasMaxLength(16);

            modelBuilder.Entity<DocumentEntity>().Property(e => e.DocumentId)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName,
                    new IndexAnnotation(new[]
                    {
                        new IndexAttribute("DocumentIdIndex") { IsUnique = true }
                    }));

            modelBuilder.Entity<DocumentEntity>().ToTable("Documents");


            // Event
            modelBuilder.Entity<EventEntity>().HasKey(e => e.Id);
            modelBuilder.Entity<EventEntity>().Property(e => e.Details).HasMaxLength(500);
            modelBuilder.Entity<EventEntity>().HasRequired(p => p.Document);
            
            modelBuilder.Entity<EventEntity>().ToTable("Events");
        }

        protected override void Dispose(bool disposing)
        {
            Log.Information("Disposing 'StoreAndForwardDbContext'");

            base.Dispose(disposing);
        }
    }
}
