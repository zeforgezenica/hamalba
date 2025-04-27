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
        public DbSet<Korisnik> Korisnici { get; set; }
        public DbSet<Recenzija> Recenzije { get; set; }

         public DbSet<Poruka> Poruke { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Additional configuration for the Oglas entity
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

            
            builder.Entity<Recenzija>(entity =>
            {
                entity.ToTable("Recenzije");
                entity.HasKey(e => e.RecenzijaId);
                entity.Property(e => e.Ocjena).IsRequired();
                entity.Property(e => e.DatumKreiranja).IsRequired();
                entity.Property(e => e.Tip).IsRequired();
                entity.Property(e => e.Komentar).HasMaxLength(500);

                entity.HasOne(r => r.Oglas)
                      .WithMany()
                      .HasForeignKey(r => r.OglasId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Autor)
                      .WithMany(k => k.DaneRecenzije)
                      .HasForeignKey(r => r.AutorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Primaoc)
                      .WithMany(k => k.PrimljeneRecenzije)
                      .HasForeignKey(r => r.PrimaocId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}