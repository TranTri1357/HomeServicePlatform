using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerIncome
{
    /// <summary>
    /// Ví/thu nhập của thợ đang đăng nhập: số dư + lịch sử các lần ghi có thu nhập
    /// (mỗi đơn 1 dòng: giá gộp → hoa hồng → thực nhận), phân trang.
    /// </summary>
    public record GetTaskerIncomeQuery(long TaskerId, int Page = 1, int PageSize = 20)
        : IRequest<ApiResponse<TaskerIncomeDto>>;
}
