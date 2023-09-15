using Harmony.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Persistence.DbContext
{
    public class HarmonyContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Board> Boards { get; set; }
        public DbSet<BoardList> BoardLists { get; set; }

        public HarmonyContext(DbContextOptions<HarmonyContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(HarmonyContext).Assembly);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>().HaveColumnType("date");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ProcessSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ProcessSaveChanges()
        {
            foreach (var item in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity is IAuditableEntity))
            {
                var entity = item.Entity as IAuditableEntity;
                entity.DateCreated = DateTime.Now;
            }

            foreach (var item in ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified && e.Entity is IAuditableEntity))
            {
                var entity = item.Entity as IAuditableEntity;
                entity.DateUpdated = DateTime.Now;
                item.Property(nameof(entity.DateCreated)).IsModified = false;
            }
        }
    }
}
