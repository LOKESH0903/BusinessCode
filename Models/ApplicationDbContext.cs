using SRRAMOils.Models;
using System.Data.Entity;

namespace SRRAMOils.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("name=DevConnection")
        {
        }

        public DbSet<Vendor> Vendors => Set<Vendor>();
        public DbSet<Organization> Organizations => Set<Organization>();
    }
}
