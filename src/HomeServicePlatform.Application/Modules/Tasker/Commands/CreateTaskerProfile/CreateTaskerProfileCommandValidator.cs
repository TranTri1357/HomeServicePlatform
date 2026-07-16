using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.CreateTaskerProfile
{
    public class CreateTaskerProfileCommandValidator : AbstractValidator<CreateTaskerProfileCommand>
    {
        public CreateTaskerProfileCommandValidator()
        {
            RuleFor(x => x.Bio)
                .NotEmpty().WithMessage("Vui lòng nhập giới thiệu bản thân.")
                .MaximumLength(1000).WithMessage("Giới thiệu không vượt quá 1000 ký tự.");

            RuleFor(x => x.ExperienceYears)
                .GreaterThanOrEqualTo(0).WithMessage("Số năm kinh nghiệm không hợp lệ.");

            // Ảnh giấy tờ bắt buộc: admin cần bằng chứng để đối chiếu, không có ảnh thì
            // hồ sơ không được tạo ⇒ không vào hàng chờ duyệt của admin.
            RuleFor(x => x.VerificationImageUrl)
                .NotEmpty().WithMessage("Vui lòng tải lên ảnh giấy tờ (CCCD/chứng chỉ) để xác minh.")
                .MaximumLength(500).WithMessage("Đường dẫn ảnh không hợp lệ.");

            // Toạ độ bắt buộc: thuật toán tìm thợ theo bán kính dựa hoàn toàn vào đây.
            // Gộp một rule cho cả cặp để thiếu vị trí chỉ báo một dòng, không lặp hai lần.
            RuleFor(x => x)
                .Must(x => x.Latitude.HasValue && x.Longitude.HasValue)
                .WithMessage("Vui lòng cung cấp vị trí làm việc.");

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Vĩ độ không hợp lệ.")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Kinh độ không hợp lệ.")
                .When(x => x.Longitude.HasValue);
        }
    }
}
