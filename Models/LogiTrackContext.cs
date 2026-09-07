// Step 4: Connect to a Database with EF Core

using Microsoft.EntityFrameworkCore;
using MSFD_LogiTrack.Models;

namespace MSFD_LogiTrack.Models
{
    public class LogiTrackContext : DbContext
    {
        public LogiTrackContext(DbContextOptions<LogiTrackContext> options) : base(options)
        {
        }

        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<Order> Orders { get; set; }

    }
}