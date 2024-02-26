using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards.Entities.Configurations
{
    public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
    {
        public void Configure(EntityTypeBuilder<WorkItem> eb)
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
        }
    }
}
