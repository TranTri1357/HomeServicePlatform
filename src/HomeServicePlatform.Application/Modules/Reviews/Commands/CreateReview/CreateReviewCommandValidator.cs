using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
    {
        public CreateReviewCommandValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween((short)1, (short)5)
                .WithMessage("Điểm đánh giá phải nằm trong khoảng từ 1 đến 5 sao.");

            RuleFor(x => x.Comment)
                .MaximumLength(500)
                .WithMessage("Bình luận không được vượt quá 500 ký tự.");
        }
    }
}
