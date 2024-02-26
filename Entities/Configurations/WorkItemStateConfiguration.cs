using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards.Entities.Configurations
{
    public class WorkItemStateConfiguration : IEntityTypeConfiguration<WorkItemState>
    {
        public void Configure(EntityTypeBuilder<WorkItemState> eb)
        {
            eb.Property(s => s.Value)
                .IsRequired()
                .HasMaxLength(60);

            eb.HasData(
                new WorkItemState() { Id = 1, Value = "To do" },
                new WorkItemState() { Id = 2, Value = "Doing" },
                new WorkItemState() { Id = 3, Value = "Done" }
                );
        }
    }
}
