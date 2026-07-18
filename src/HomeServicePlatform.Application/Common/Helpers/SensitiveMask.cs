using System;
using System.Linq;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// Che bớt dữ liệu định danh trước khi ghi xuống DB hoặc trả về client.
    ///
    /// Nguyên tắc: số tài khoản và số điện thoại nhận tiền chỉ cần đủ để người dùng NHẬN RA
    /// tài khoản của mình, không cần đủ để người khác dùng lại. Vì vậy hệ thống chỉ lưu vài
    /// chữ số cuối — kể cả khi log hay lịch sử giao dịch bị lộ thì cũng không tái sử dụng được.
    /// </summary>
    public static class SensitiveMask
    {
        /// <summary>Giữ lại <paramref name="visible"/> ký tự cuối, phần còn lại thay bằng dấu *.</summary>
        public static string Tail(string? value, int visible = 4)
        {
            var digits = new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());

            if (digits.Length == 0) return "****";
            if (digits.Length <= visible) return new string('*', 4) + digits;

            return new string('*', 4) + digits[^visible..];
        }
    }
}
