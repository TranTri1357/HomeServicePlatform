using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers
{
    /// <summary>
    /// Lấy đúng thẻ của MỘT thợ cho một dịch vụ (cùng shape với danh sách gợi ý). Dùng để
    /// GHIM thẻ thợ khách chọn sẵn (khi vào đặt lịch từ trang hồ sơ thợ) lên đầu — thợ đó có
    /// thể xếp hạng thấp, nằm ở trang sau nên không lọt vào 5 người đầu.
    /// </summary>
    public record GetServiceTaskerCardQuery(long ServiceId, long TaskerId)
        : IRequest<ApiResponse<ServiceTaskerSuggestionDto?>>;
}
