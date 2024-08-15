using Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data.Context
{
    public class DatabaseContext : DbContext
    {

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<EmployeeItem> EmployeeItems => Set<EmployeeItem>();
        public DbSet<PaymentHistory> PaymentHistories { get; set; }
        public DbSet<ChangeLogEmployee> ChangeLogEmployees { get; set; }
        public DbSet<ChangeLogItem> ChangeLogItems { get; set; }
        public DbSet<ChangeLogEmployeeItem> ChangeLogEmployeeItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // EmployeeItem için ilişkileri tanımla
            modelBuilder.Entity<EmployeeItem>()
                .HasKey(ei => ei.EmployeeItemId);

            modelBuilder.Entity<EmployeeItem>()
                .HasOne(ei => ei.Employee)
                .WithMany(e => e.EmployeeItems)
                .HasForeignKey(ei => ei.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeeItem>()
                .HasOne(ei => ei.Item)
                .WithMany(i => i.EmployeeItems)
                .HasForeignKey(ei => ei.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // Decimal özellikler için hassasiyet ve ölçek ayarlarını belirle
            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<EmployeeItem>()
                .Property(ei => ei.PaidAmount)
                .HasColumnType("decimal(18,2)");
        }






    }
}
