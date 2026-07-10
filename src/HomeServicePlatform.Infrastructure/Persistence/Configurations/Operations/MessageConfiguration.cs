using HomeServicePlatform.Domain.Modules.Operations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Operations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> entity)
        {
            entity.ToTable("messages");

            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.MessageId).HasColumnName("message_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Content).HasColumnName("content").HasMaxLength(2000).IsRequired();
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Booking).WithMany().HasForeignKey(d => d.BookingId).HasConstraintName("fk_messages_booking");
            entity.HasOne(d => d.Sender).WithMany().HasForeignKey(d => d.SenderId).HasConstraintName("fk_messages_sender");

            entity.HasIndex(e => new { e.BookingId, e.CreatedAt }).HasDatabaseName("ix_messages_booking_id_created_at");
            entity.HasIndex(e => e.SenderId).HasDatabaseName("ix_messages_sender_id");
        }
    }
}
