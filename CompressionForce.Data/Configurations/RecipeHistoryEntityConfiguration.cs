using CompressionForce.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompressionForce.Data.Configurations
{
    public class RecipeHistoryEntityConfiguration : IEntityTypeConfiguration<RecipeHistoryEntity>
    {
        public void Configure(EntityTypeBuilder<RecipeHistoryEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.RecipeCode);

            builder.Property(x => x.OldParameters)
                   .HasColumnType("jsonb");

            builder.Property(x => x.NewParameters)
                   .HasColumnType("jsonb");
        }
    }
}
