namespace WarehouseApi.Common.PureCqrs
{
    public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken token = default);
    }
}
