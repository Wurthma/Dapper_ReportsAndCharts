using Microsoft.AspNet.Identity.EntityFramework;

namespace DapperGraphs.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<Country> Countries { get; set; }
        public System.Data.Entity.DbSet<EnergyUsageData> EnergyUsageDatas { get; set; }
    }
}