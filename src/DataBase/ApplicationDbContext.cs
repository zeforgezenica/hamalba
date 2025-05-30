using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using hamalba.Models;
using Microsoft.AspNetCore.Identity;

namespace hamalba.DataBase
{
    public class ApplicationDbContext : IdentityDbContext<Korisnik>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Oglas> Oglasi { get; set; }
        public DbSet<KorisnikOglas> KorisnikOglasi { get; set; }
        public DbSet<Kontakt> Kontakt { get; set; }
        public DbSet<Poruka> Poruke { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Korisnik> Korisnici { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Oglas>(entity =>
            {
                entity.ToTable("Oglasi");
                entity.HasKey(e => e.OglasId);
                entity.Property(e => e.Naslov).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Opis).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Datum).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Rok).IsRequired();
                entity.Property(e => e.Cijena).IsRequired();
                entity.Property(e => e.Lokacija).IsRequired().HasMaxLength(255);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId);
            });


            builder.Entity<Report>(entity =>
            {
                entity.HasOne(r => r.Reporter)
                      .WithMany()
                      .HasForeignKey(r => r.ReporterUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.ReportedUser)
                      .WithMany()
                      .HasForeignKey(r => r.ReportedUserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
