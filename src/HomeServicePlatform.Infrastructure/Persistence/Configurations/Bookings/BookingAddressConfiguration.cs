using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Bookings
{
    public class BookingAddressConfiguration : IEntityTypeConfiguration<BookingAddress>
    {
        public void Configure(EntityTypeBuilder<BookingAddress> entity)
        {
            entity.ToTable("booking_addresses");

            entity.HasKey(e => e.BookingAddressId);
            entity.Property(e => e.BookingAddressId).HasColumnName("booking_address_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            entity.Property(e => e.ProvinceCode).HasColumnName("province_code").HasMaxLength(20);
            entity.Property(e => e.DistrictCode).HasColumnName("district_code").HasMaxLength(20);
            entity.Property(e => e.WardCode).HasColumnName("ward_code").HasMaxLength(20);
            entity.Property(e => e.AddressLine).HasColumnName("address_line").HasMaxLength(300).IsRequired();
            entity.Property(e => e.Geom).HasColumnName("geom");

            entity.HasOne(d => d.Booking).WithOne(p => p.BookingAddress).HasForeignKey<BookingAddress>(d => d.BookingId).HasConstraintName("fk_booking_addresses_booking");
        }
    }
}
