using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Persistence.DbContext
{
    /// <summary>
    /// The DbContext for Harmony
    /// </summary>
    public class HarmonyContext : AppIdentityContext
    {
        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<UserWorkspace> UserWorkspaces { get; set; }

        public DbSet<Board> Boards { get; set; }
        public DbSet<UserBoard> UserBoards { get; set; }
        public DbSet<BoardList> BoardLists { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<UserCard> UserCards { get; set; }
        public DbSet<CheckList> CheckLists { get; set; }
        public DbSet<CheckListItem> CheckListItems { get; set; }
        public DbSet<Label> Labels { get; set; }
        public DbSet<CardLabel> CardLabels { get; set; }
        public DbSet<CardActivity> CardActivities { get; set; }
        public DbSet<IssueType> IssueTypes { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

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
            // configurationBuilder.Properties<DateTime>().HaveColumnType("date");
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
