using CompressionForce.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompressionForce.Data.Configurations
{
    public class RecipeEntityConfiguration : IEntityTypeConfiguration<RecipeEntity>
    {
        public void Configure(EntityTypeBuilder<RecipeEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.RecipeCode)
                   .IsUnique();

            builder.Property(x => x.Parameters)
                   .HasColumnType("jsonb")
                   .IsRequired();
        }
    }
}
