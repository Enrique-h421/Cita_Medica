using Domain.AgendaYAtencion;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class ClinicaDbContext : DbContext
    {
       
        public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options) { }

       
        public DbSet<Cita> Citas { get; set; }
        public DbSet<EstadoCita> Estados_Cita { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<EstadoCita>().HasKey(e => e.EstadoCitaID);
            modelBuilder.Entity<Cita>().HasKey(c => c.CitaID);

            
            modelBuilder.Entity<Cita>()
                .HasOne(c => c.EstadoCita)
                .WithMany(e => e.Citas)
                .HasForeignKey(c => c.EstadoCitaID);
        }
    }
}