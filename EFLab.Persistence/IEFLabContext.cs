using System;
using System.Threading;
using System.Threading.Tasks;
using EFLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EFLab.Persistence
{
    public interface IEFLabContext
    {
        DbSet<Group> Groups { get; set; }

        DbSet<User> Users { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
