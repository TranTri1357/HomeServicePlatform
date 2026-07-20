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
    /// <remarks>
    /// CustomerLat/Lng chỉ để tính khoảng cách hiển thị, giữ thẻ ghim đồng nhất với các thẻ
    /// trong danh sách. Thẻ ghim KHÔNG lọc theo tỉnh: thợ này do chính khách chọn từ trang
    /// hồ sơ, chặn ở đây sẽ làm thẻ biến mất một cách khó hiểu — cứ hiện kèm khoảng cách để
    /// khách tự thấy xa mà cân nhắc.
    /// </remarks>
    public record GetServiceTaskerCardQuery(
        long ServiceId,
        long TaskerId,
        double? CustomerLat = null,
        double? CustomerLng = null)
        : IRequest<ApiResponse<ServiceTaskerSuggestionDto?>>;
}
