using hamalba.Models;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using hamalba.Models;

namespace hamalba.DataBase
{
    public class ApplicationDbContext : IdentityDbContext<Korisnik>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Oglas> Oglasi { get; set; }

        
    }

}
