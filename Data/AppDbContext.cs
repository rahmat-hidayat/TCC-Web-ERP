using Microsoft.EntityFrameworkCore;
using TCC_Web_ERP.Models;

namespace TCC_Web_ERP.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected AppDbContext() : this(new DbContextOptions<AppDbContext>())
        {
        }

        // Tambahkan DbSet untuk tabel-tabel yang kamu punya
        public DbSet<TUser> TUSER { get; set; }
        public DbSet<TMenu> TMENU { get; set; }
        public DbSet<TRole> TROLE { get; set; }
        public DbSet<TRoleMenu> TROLEMENU { get; set; }

        public DbSet<TRoleMenu> ROLE_MENU { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TRoleMenu>()
                .HasKey(rm => new { rm.RoleId, rm.MenuId });

            modelBuilder.Entity<TRoleMenu>()
                .HasOne(rm => rm.Role)
                .WithMany(r => r.RoleMenus)
                .HasForeignKey(rm => rm.RoleId);

            modelBuilder.Entity<TRoleMenu>()
                .HasOne(rm => rm.Menu)
                .WithMany(m => m.RoleMenus)
                .HasForeignKey(rm => rm.MenuId);
        }
    }
}


