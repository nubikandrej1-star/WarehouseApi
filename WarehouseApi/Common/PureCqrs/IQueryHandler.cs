namespace WarehouseApi.Common.PureCqrs
{
    public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
    {
        Task<TResult> HandleAsync(TQuery command, CancellationToken token = default);
    }
}
