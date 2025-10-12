namespace RoomEnglish.Application.Authentication.Commands.GetToken;

public class GetTokenCommandValidator : AbstractValidator<GetTokenCommand>
{
    public GetTokenCommandValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty()
            .WithMessage("Email là bắt buộc.")
            .EmailAddress()
            .WithMessage("Email không đúng định dạng.");

        RuleFor(v => v.Password)
            .NotEmpty()
            .WithMessage("Password là bắt buộc.")
            .MinimumLength(6)
            .WithMessage("Password phải có ít nhất 6 ký tự.");
    }
}