using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Domain.Modules.Tasker.Entities
{
    public class TaskerProfile
    {
        public long TaskerProfileId { get; set; }
        public string? Bio { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTimeOffset? VerifiedAt { get; set; }
        public int ExperienceYears { get; set; } = 0;
        public Point? CurrentGeom { get; set; }

        public string? VerificationImageUrl { get; set; }

        public string? RejectionReason { get; set; }
        public decimal RatingAvg { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public short Status { get; set; } = 0;
        public bool IsDeleted { get; set; } = false;

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

        public void RejectProfile(string? reason)
        {
            Status = 4;
            RejectionReason = reason;
        }

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
            Status = 2;
        }

        public void RecordCancellation()
        {
            CancelCount++;
        }

        public void RecordCompletion()
        {
            CompletedCount++;
        }

        public void UpdateLocation(Point newGeom)
        {
            CurrentGeom = newGeom ?? throw new ArgumentNullException(nameof(newGeom));
        }

        public void UpdateRating(short newRating)
        {
            if (newRating < 1 || newRating > 5)
                throw new ArgumentOutOfRangeException(nameof(newRating), "Điểm đánh giá phải từ 1 đến 5.");

            decimal totalScore = (RatingAvg * TotalReviews) + newRating;
            TotalReviews++;
            RatingAvg = Math.Round(totalScore / TotalReviews, 1);
        }
    }


}
