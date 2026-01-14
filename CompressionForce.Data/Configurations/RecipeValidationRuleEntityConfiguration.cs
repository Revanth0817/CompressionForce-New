
using CompressionForce.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CompressionForce.Data.Configurations
{

    // CompressionForce.Data/Configurations/RecipeValidationRuleEntityConfiguration.cs
    public class RecipeValidationRuleEntityConfiguration : IEntityTypeConfiguration<RecipeValidationRuleEntity>
    {
        public void Configure(EntityTypeBuilder<RecipeValidationRuleEntity> builder)
        {
            builder.ToTable("RecipeValidationRules");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Type).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Required).HasDefaultValue(false);
            builder.Property(x => x.LookupCategory).HasMaxLength(100);
            builder.HasIndex(x => x.Name).IsUnique();
        }
    }

}


/*When you move to DB-backed validation, 
 add modelBuilder.ApplyConfiguration(new RecipeValidationRuleEntityConfiguration()); 
in ApplicationDbContext.OnModelCreating.
*/