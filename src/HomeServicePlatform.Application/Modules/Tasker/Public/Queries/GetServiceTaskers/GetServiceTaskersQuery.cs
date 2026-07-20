using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers
{
    /// <summary>
    /// Danh sách thợ nhận một dịch vụ, SẮP THEO ĐÁNH GIÁ + PHÂN TRANG "tải thêm".
    /// Tách khỏi <c>GetServiceDetail</c> (vốn chỉ trả top 5 để xem nhanh) để khách xem
    /// được toàn bộ thợ khi bấm "Xem thêm".
    ///
    /// <para><b>ProvinceCode</b> — mã tỉnh/thành của địa chỉ khách đang đặt. Khi có, chỉ trả
    /// thợ CÙNG TỈNH. Nếu không lọc, khách Cà Mau vẫn thấy thợ TP.HCM, chọn nhầm rồi mới
    /// phát hiện thợ ở quá xa. Null (khách chưa chọn địa chỉ đã lưu, hoặc khách vãng lai
    /// đang xem dạo) thì giữ nguyên hành vi cũ: trả tất cả thợ.</para>
    ///
    /// <para><b>CustomerLat/Lng</b> — chỉ dùng để TÍNH khoảng cách hiển thị, không lọc.</para>
    /// </summary>
    public record GetServiceTaskersQuery(
        long ServiceId,
        int PageIndex = 1,
        int PageSize = 5,
        string? ProvinceCode = null,
        double? CustomerLat = null,
        double? CustomerLng = null)
        : IRequest<ApiResponse<PagedResult<ServiceTaskerSuggestionDto>>>;
}
