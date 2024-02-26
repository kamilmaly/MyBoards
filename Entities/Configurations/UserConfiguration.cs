using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MyBoards.Entities.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> eb)
        {
            eb.HasOne(u => u.Address)
              .WithOne(a => a.User)
              .HasForeignKey<Address>(a => a.UserId);

            eb.HasMany(w => w.Comments)
            .WithOne(u => u.Author)
            .HasForeignKey(w => w.AuthorId)
            .OnDelete(DeleteBehavior.ClientCascade);

            eb.HasIndex(u => new { u.Email, u.FullName });
        }
    }
}
