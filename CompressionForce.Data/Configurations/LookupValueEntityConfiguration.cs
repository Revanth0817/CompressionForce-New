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
    public class LookupValueEntityConfiguration : IEntityTypeConfiguration<LookupValueEntity>
    {
        public void Configure(EntityTypeBuilder<LookupValueEntity> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.Category, x.Code })
                   .IsUnique();

            builder.Property(x => x.IsActive)
                   .HasDefaultValue(true);
        }
    }
}
