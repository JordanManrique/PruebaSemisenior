using back_prueba.Entities;
using Microsoft.EntityFrameworkCore;

namespace back_prueba.Data
{
    public class LabDbContext(DbContextOptions<LabDbContext> options) : DbContext(options)
    {
        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<LabTest> Tests => Set<LabTest>();
        public DbSet<Sample> Samples => Set<Sample>();
        public DbSet<SampleTest> SampleTests => Set<SampleTest>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Patient>(e =>
            {
                e.ToTable("Patients");
                e.Property(p => p.DocumentNumber).HasMaxLength(12).IsUnicode(false);
                e.Property(p => p.FullName).HasMaxLength(150);
                e.Property(p => p.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                e.HasIndex(p => p.DocumentNumber).IsUnique();
            });

            modelBuilder.Entity<LabTest>(e =>
            {
                e.ToTable("Tests");
                e.Property(t => t.Code).HasMaxLength(20).IsUnicode(false);
                e.Property(t => t.Name).HasMaxLength(100);
                e.HasIndex(t => t.Code).IsUnique();
            });

            modelBuilder.Entity<Sample>(e =>
            {
                e.ToTable("Samples");
                e.Property(s => s.SampleType).HasMaxLength(20).IsUnicode(false);
                e.Property(s => s.Status).HasMaxLength(20).IsUnicode(false);
                e.Property(s => s.RejectionReason).HasMaxLength(500);
                e.Property(s => s.CreatedAt).HasDefaultValueSql("SYSDATETIME()");
                e.HasOne(s => s.Patient).WithMany(p => p.Samples).HasForeignKey(s => s.PatientId);
            });

            modelBuilder.Entity<SampleTest>(e =>
            {
                e.ToTable("SampleTests");
                e.HasKey(st => new { st.SampleId, st.TestId });
                e.HasOne(st => st.Sample).WithMany(s => s.SampleTests).HasForeignKey(st => st.SampleId);
                e.HasOne(st => st.Test).WithMany(t => t.SampleTests).HasForeignKey(st => st.TestId);
            });
        }
    }
}
