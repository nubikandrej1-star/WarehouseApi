using FluentValidation;

namespace WarehouseApi.Features.Shipments.Commands.UpdateShipmentStatus;

public sealed class UpdateShipmentStatusValidator : AbstractValidator<UpdateShipmentStatusCommand>
{
    private static readonly string[] AllowedStatuses = ["InTransit", "Delivered", "Cancelled"];

    public UpdateShipmentStatusValidator()
    {
        RuleFor(command => command.ShipmentId)
            .GreaterThan(0).WithMessage("ShipmentId має бути більшим за 0.");

        RuleFor(command => command.NewStatus)
            .Must(status => !string.IsNullOrWhiteSpace(status) && AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Новий статус має бути одним із: InTransit, Delivered, Cancelled.")
            .Must(status => status is null || status == status.Trim())
            .WithMessage("Статус не повинен містити пробілів на початку або в кінці.");
    }
}
