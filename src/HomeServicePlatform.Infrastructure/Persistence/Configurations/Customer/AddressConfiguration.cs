using HomeServicePlatform.Domain.Modules.Customer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Customer
{
    public class AddressConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> entity)
        {
            entity.ToTable("addresses");

            entity.HasKey(e => e.AddressId);
            entity.Property(e => e.AddressId).HasColumnName("address_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ProvinceCode).HasColumnName("province_code").HasMaxLength(20);
            entity.Property(e => e.DistrictCode).HasColumnName("district_code").HasMaxLength(20);
            entity.Property(e => e.WardCode).HasColumnName("ward_code").HasMaxLength(20);
            entity.Property(e => e.AddressLine).HasColumnName("address_line").HasMaxLength(300).IsRequired();
            entity.Property(e => e.Geom).HasColumnName("geom");
            entity.Property(e => e.IsDefault).HasColumnName("is_default").HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.Addresses).HasForeignKey(d => d.UserId).HasConstraintName("fk_addresses_user");

            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_addresses_user_id");
            entity.HasIndex(e => e.Geom).HasMethod("gist").HasDatabaseName("gist_addresses_geom");
        }
    }
}
