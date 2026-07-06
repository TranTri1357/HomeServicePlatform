using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Dtos
{
    public record TaskerDto(
        long TaskerId,
        string FullName,
        string Phone,
        List<string> Skills,  
        decimal RatingAvg,    
        int TotalJobs,        
        short Status,
        DateTimeOffset JoinedDate
    );
}
