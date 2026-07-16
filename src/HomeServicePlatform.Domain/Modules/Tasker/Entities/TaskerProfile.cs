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

        // Ảnh giấy tờ (CCCD/chứng chỉ) thợ nộp để admin đối chiếu trước khi duyệt.
        // Nullable vì các hồ sơ tạo trước tính năng này không có ảnh; hồ sơ mới bị
        // validator chặn nếu thiếu.
        public string? VerificationImageUrl { get; set; }

        // Lý do admin từ chối, để thợ biết phải sửa gì mà nộp lại.
        public string? RejectionReason { get; set; }
        public decimal RatingAvg { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public short Status { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;

        // 📊 Độ tin cậy (tách biệt hoàn toàn với RatingAvg - vốn chỉ phản ánh tay nghề
        // qua đánh giá của khách). Hai chỉ số này KHÔNG trộn lẫn.
        // CancelCount: tổng số lần THỢ chủ động hủy/không thực hiện.
        // CompletedCount: tổng số đơn thợ đã hoàn thành.
        // Reliability% (tính khi cần) = Completed / (Completed + Cancel) * 100.
        public int CancelCount { get; set; } = 0;
        public int CompletedCount { get; set; } = 0;

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
            RejectionReason = null;
        }

        /// <summary>Admin từ chối hồ sơ kèm lý do để thợ biết đường sửa và nộp lại.</summary>
        public void RejectProfile(string? reason)
        {
            Status = 2;
            RejectionReason = reason;
        }

        /// <summary>
        /// Thợ nộp lại hồ sơ đã bị từ chối: cập nhật nội dung mới rồi đưa về hàng chờ duyệt.
        /// Không có bước này thì hồ sơ bị từ chối sẽ kẹt vĩnh viễn ở Status=2.
        /// </summary>
        public void ResubmitForApproval(string? bio, int experienceYears, Point currentGeom, string verificationImageUrl)
        {
            Bio = bio;
            ExperienceYears = experienceYears;
            CurrentGeom = currentGeom;
            VerificationImageUrl = verificationImageUrl;
            Status = 0;
            RejectionReason = null;
        }


        public void SuspendTasker()
        {
            Status = 0;
                        // Có thể thêm logic: Ghi log lý do khóa ở đây nếu bạn có bảng Log
        }

        /// <summary>Ghi nhận 1 lần thợ hủy đơn (hạ độ tin cậy, KHÔNG đụng tới RatingAvg).</summary>
        public void RecordCancellation()
        {
            CancelCount++;
        }

        /// <summary>Ghi nhận 1 đơn hoàn thành (tăng độ tin cậy).</summary>
        public void RecordCompletion()
        {
            CompletedCount++;
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
