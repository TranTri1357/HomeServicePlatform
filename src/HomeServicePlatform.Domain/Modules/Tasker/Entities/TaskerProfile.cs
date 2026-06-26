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
    }
}
