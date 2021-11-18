using System.Threading;
using System.Threading.Tasks;
using EFLab.Domain.Entities;
using EFLab.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EntityFramework.API.Entities
{
    public class EFLabContext : DbContext, IEFLabContext
    {
        public EFLabContext(DbContextOptions<EFLabContext> options) : base(options)
        {}
      
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(EFLabContext).Assembly);
        }
    }
}

