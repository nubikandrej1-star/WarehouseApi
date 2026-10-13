using FluentValidation;

namespace WarehouseApi.Features.Shipments.Commands.CreateShipment;

public sealed class CreateShipmentValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentValidator()
    {
        RuleFor(command => command.RecipientName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Ім'я отримувача є обов'язковим.")
            .MinimumLength(3).WithMessage("Ім'я отримувача має містити щонайменше 3 символи.")
            .MaximumLength(150).WithMessage("Ім'я отримувача не може перевищувати 150 символів.");

        RuleFor(command => command.DestinationAddress)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Адреса доставки є обов'язковою.")
            .MinimumLength(5).WithMessage("Адреса доставки має містити щонайменше 5 символів.")
            .MaximumLength(300).WithMessage("Адреса доставки не може перевищувати 300 символів.");

        RuleFor(command => command.Quantity)
            .GreaterThan(0).WithMessage("Кількість товару має бути більшою за 0.");

        RuleFor(command => command.ProductId)
            .GreaterThan(0).WithMessage("ProductId має бути більшим за 0.");
    }
}
