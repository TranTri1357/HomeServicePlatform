using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Domain.Modules.Tasker.Entities
{
    public class TaskerProfile
    {
        public long TaskerProfileId { get; set; } // PK đồng thời là FK sang Users
        public string? Bio { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTimeOffset? VerifiedAt { get; set; }
        public int ExperienceYears { get; set; } = 0;
        public Point? CurrentGeom { get; set; }
        public decimal RatingAvg { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public short Status { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;

        public virtual User User { get; set; } = null!;
        public virtual ICollection<TaskerService> TaskerServices { get; set; } = new List<TaskerService>();
        public virtual ICollection<TaskerSchedule> TaskerSchedules { get; set; } = new List<TaskerSchedule>();
        public virtual ICollection<TaskerTimeOff> TaskerTimeOffs { get; set; } = new List<TaskerTimeOff>();
        public virtual ICollection<TaskerServicePrice> TaskerServicePrices { get; set; } = new List<TaskerServicePrice>();


        public void VerifyTasker()
        {
            if (IsVerified)
                throw new InvalidOperationException("Tài khoản thợ này đã được xác thực trước đó.");

            IsVerified = true;
            VerifiedAt = DateTimeOffset.UtcNow;
            Status = 1;
        }


        public void SuspendTasker()
        {
            Status = 0;
                        // Có thể thêm logic: Ghi log lý do khóa ở đây nếu bạn có bảng Log
        }

        /// <summary>
        /// Cập nhật vị trí GPS mới nhất của thợ (Dùng cho App của thợ bắn tọa độ liên tục)
        /// </summary>
        public void UpdateLocation(Point newGeom)
        {
            CurrentGeom = newGeom ?? throw new ArgumentNullException(nameof(newGeom));
        }

        /// <summary>
        /// Cập nhật điểm đánh giá trung bình sau khi có review mới
        /// </summary>
        public void UpdateRating(short newRating)
        {
            if (newRating < 1 || newRating > 5)
                throw new ArgumentOutOfRangeException(nameof(newRating), "Điểm đánh giá phải từ 1 đến 5.");

            // Công thức: ((Điểm TB cũ * Tổng số đánh giá cũ) + Điểm mới) / Tổng số đánh giá mới
            decimal totalScore = (RatingAvg * TotalReviews) + newRating;
            TotalReviews++;
            RatingAvg = Math.Round(totalScore / TotalReviews, 1); // Làm tròn 1 chữ số thập phân
        }
    }


}
