using Microsoft.EntityFrameworkCore;

namespace EFLab.Persistence
{
    public class EFLabContextFactory : DesignTimeDbContextFactoryBase<EFLabContext>
    {
        protected override EFLabContext CreateNewInstance(DbContextOptions<EFLabContext> options)
        {
            return new EFLabContext(options);
        }
    }
}
