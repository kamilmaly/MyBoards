using Microsoft.EntityFrameworkCore;

namespace MyBoards.Entities
{
    public class MyBoardsContext : DbContext
    {
        public MyBoardsContext(DbContextOptions<MyBoardsContext> options) : base(options)
        {

        }
        public DbSet<WorkItem> WorkItems { get; set; }
        public DbSet<Issue> Issues { get; set; }
        public DbSet<Epic> Epics { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<WorkItemState> WorkItemStates { get; set; }
        public DbSet<WorkItemTag> WorkItemTag { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Epic>(eb =>
            {
                eb.Property(wi => wi.EndDate)
                .HasPrecision(3);
            });

            modelBuilder.Entity<Task>(eb =>
            {
                eb.Property(wi => wi.Activity)
                .HasMaxLength(200);
                eb.Property(wi => wi.RemaningWork)
                .HasPrecision(14, 2);
            });

            modelBuilder.Entity<Issue>(eb =>
            {
                eb.Property(wi => wi.Efford)
                .HasColumnType("decimal(5,2)");
            });

            modelBuilder.Entity<WorkItem>(eb =>
            {
                eb.HasOne(s => s.State)
                .WithMany()
                .HasForeignKey(s => s.StateId);

                eb.Property(wi => wi.Area).HasColumnType("varchar(200)");
                eb.Property(wi => wi.IterationPath).HasColumnName("Iteration_Path");
                eb.Property(wi => wi.Priority).HasDefaultValue(3);
                eb.HasMany(w => w.Comments)
                .WithOne(c => c.WorkItem)
                .HasForeignKey(c => c.WorkItemId);

                eb.HasOne(w => w.Author)
                .WithMany(u => u.WorkItems)
                .HasForeignKey(w => w.AuthorId);

                eb.HasMany(w => w.Tags)
                 .WithMany(t => t.WorkItems)
                 .UsingEntity<WorkItemTag>(
                    w => w.HasOne(wit => wit.Tag)
                    .WithMany()
                    .HasForeignKey(wit => wit.TagId),

                    w => w.HasOne(wit => wit.WorkItem)
                    .WithMany()
                    .HasForeignKey(wit => wit.WorkItemId),

                    wit =>
                    {
                        wit.HasKey(x => new { x.TagId, x.WorkItemId });
                        wit.Property(x => x.PublicationDate).HasDefaultValueSql("getutcdate()");
                    });

            });

            modelBuilder.Entity<Comment>(eb =>
            {
                eb.Property(x => x.CreatedDate).HasDefaultValueSql("getutcdate()");
                eb.Property(x => x.UpdatedDate).ValueGeneratedOnUpdate();
            });

            modelBuilder.Entity<User>(eb =>
            {
                eb.HasOne(u => u.Address)
                 .WithOne(a => a.User)
                 .HasForeignKey<Address>(a => a.UserId);

                eb.HasMany(w => w.Comments)
                .WithOne(u => u.Author)
                .HasForeignKey(w => w.AuthorId)
                .OnDelete(DeleteBehavior.NoAction);
            });


            modelBuilder.Entity<WorkItemState>(eb =>
            {
                eb.Property(s => s.Value)
                .IsRequired()
                .HasMaxLength(60);

                eb.HasData(
                    new WorkItemState() { Id = 1, Value = "To do" },
                    new WorkItemState() { Id = 2, Value = "Doing" },
                    new WorkItemState() { Id = 3, Value = "Done" }
                    );
            });

            modelBuilder.Entity<Tag>(eb =>
            {
                eb.HasData(
                    new Tag() { Id = 1, Value = "Web" },
                    new Tag() { Id = 2, Value = "UI" },
                    new Tag() { Id = 3, Value = "Desktop" },
                    new Tag() { Id = 4, Value = "API" },
                    new Tag() { Id = 5, Value = "Service" }
                    );
            });


        }
    }
}
