using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards.Entities.Configurations
{
    public class TaskConfiguration : IEntityTypeConfiguration<Task>
    {
        public void Configure(EntityTypeBuilder<Task> eb)
        {
            eb.Property(wi => wi.Activity)
               .HasMaxLength(200);
            eb.Property(wi => wi.RemaningWork)
            .HasPrecision(14, 2);
        }
    }
}
