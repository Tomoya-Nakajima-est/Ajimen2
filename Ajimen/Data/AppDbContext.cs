using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ajimen.Models;

namespace Ajimen.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Identityユーザー情報
        public DbSet<ApplicationUser> Users { get; set; }

        // 独自モデルももちろん追加OK
        public DbSet<Item> Items { get; set; }
        public DbSet<OrderLog> OrderLogs { get; set; }
        public DbSet<StockLog> StockLogs { get; set; }

        public DbSet<Attendance> Attendances { get; set; }
        
        public DbSet<Shift> Shifts { get; set; }

    }
}