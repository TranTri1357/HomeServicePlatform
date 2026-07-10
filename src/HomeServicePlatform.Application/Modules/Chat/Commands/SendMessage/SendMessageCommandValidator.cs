using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Chat.Commands.SendMessage
{
    public class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
    {
        public SendMessageCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Nội dung tin nhắn không được để trống.")
                .MaximumLength(2000).WithMessage("Tin nhắn không được vượt quá 2000 ký tự.");
        }
    }
}
