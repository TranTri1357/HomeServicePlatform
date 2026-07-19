using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;

namespace HomeServicePlatform.Domain.Modules.Bookings.Entities
{
    public class Booking
    {
        public long BookingId { get; set; }
        public long CustomerId { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public decimal SubtotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public int RowVersion { get; set; } = 1;

        // 🚨 Đơn khẩn cấp: khách gọi trực tiếp 1 thợ đang rảnh gần đó, thợ có
        // EmergencyExpiresAt (30s) để bấm nhận trước khi đơn hết hạn.
        public bool IsEmergency { get; set; } = false;
        public DateTimeOffset? EmergencyExpiresAt { get; set; }

        // Navigation Properties
        public virtual User Customer { get; set; } = null!;
        public virtual BookingAddress? BookingAddress { get; set; }
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();
        public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
        public virtual ICollection<EmergencyBookingDecline> EmergencyDeclines { get; set; } = new List<EmergencyBookingDecline>();

        /// <summary>
        /// Bước 1: Khách hàng tạo mới đơn hàng
        /// </summary>
        public void InitializeBooking(long customerId)
        {
            this.Status = BookingStatus.Pending;
            this.BookingHistories.Add(new BookingHistory
            {
                BookingId = this.BookingId,
                OldStatus = -1, // Đơn mới tinh chưa có trạng thái cũ
                NewStatus = (short)BookingStatus.Pending,
                ChangedBy = customerId,
                CreatedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Các bước cập nhật trạng thái luồng 6 bước nghiêm ngặt (State Machine)
        /// </summary>
        public void AcceptByTasker(long taskerId, string? note = null)
        {
            if (this.Status != BookingStatus.Pending)
                throw new InvalidOperationException($"Không hợp lệ. Đơn đang ở trạng thái: {this.Status}");

            UpdateStatusAndLog(BookingStatus.Accepted, taskerId, note ?? "Thợ đã bấm xác nhận đơn hàng.");

            foreach (var item in this.BookingItems)
            {
                item.TaskerId = taskerId;
                item.Status = (short)BookingStatus.Accepted;
            }
        }

        public void StartMovingByTasker(long taskerId, string? note = null)
        {
            EnsureAssignedTasker(taskerId);
            if (this.Status != BookingStatus.Accepted)
                throw new InvalidOperationException("Thợ phải xác nhận đơn trước khi báo di chuyển.");

            UpdateStatusAndLog(BookingStatus.OnTheWay, taskerId, note ?? "Thợ đang trên đường đến điểm hẹn.");
        }

        public void StartWorkingByTasker(long taskerId, string? note = null)
        {
            EnsureAssignedTasker(taskerId);
            if (this.Status != BookingStatus.OnTheWay)
                throw new InvalidOperationException("Thợ phải bấm 'Đang di chuyển' trước khi báo bắt đầu làm.");

            UpdateStatusAndLog(BookingStatus.InProgress, taskerId, note ?? "Thợ đã đến nơi và bắt đầu thực hiện.");
        }

        public void CompleteWorkAndPendingPayment(long taskerId, string? note = null)
        {
            EnsureAssignedTasker(taskerId);
            if (this.Status != BookingStatus.InProgress)
                throw new InvalidOperationException("Không thể hoàn thành dịch vụ chưa bấm bắt đầu.");

            UpdateStatusAndLog(BookingStatus.Completed, taskerId, note ?? "Công việc hoàn tất, chờ khách thanh toán và xác nhận hoàn thành.");
        }

        /// <summary>
        /// 🔒 CHỐNG IDOR: chỉ thợ ĐƯỢC GÁN vào đơn mới được đẩy trạng thái đơn đó. Chặn việc một
        /// thợ khác dò BookingId rồi thao tác lên đơn không phải của mình.
        /// </summary>
        private void EnsureAssignedTasker(long taskerId)
        {
            if (!this.BookingItems.Any(i => i.TaskerId == taskerId))
                throw new InvalidOperationException("Bạn không phụ trách đơn này nên không thể thao tác.");
        }

        //public void ConfirmPaymentAndComplete(long systemOrAdminId, string? note = null)
        //{
        //    if (this.Status != BookingStatus.PendingPayment)
        //        throw new InvalidOperationException("Đơn hàng phải ở trạng thái chờ thanh toán.");

        //    UpdateStatusAndLog(BookingStatus.Completed, systemOrAdminId, note ?? "Thanh toán thành công. Đơn hàng kết thúc.");
        //}

        private void UpdateStatusAndLog(BookingStatus newStatus, long actorId, string note)
        {
            var oldStatus = this.Status;
            this.Status = newStatus;
            this.UpdatedAt = DateTime.UtcNow;

            // 🔄 Đồng bộ trạng thái cho toàn bộ hạng mục để danh sách việc của Thợ
            // (đọc BookingItem.Status) không bị lệch với trạng thái đơn tổng.
            foreach (var item in this.BookingItems)
            {
                item.Status = (short)newStatus;
                item.UpdatedAt = DateTime.UtcNow;
            }

            this.BookingHistories.Add(new BookingHistory
            {
                BookingId = this.BookingId,
                OldStatus = (short)oldStatus, // Ép kiểu Enum về short để lưu database
                NewStatus = (short)newStatus, // Ép kiểu Enum về short để lưu database
                ChangedBy = actorId,
                CreatedAt = DateTime.UtcNow
            });
        }

    }

}

