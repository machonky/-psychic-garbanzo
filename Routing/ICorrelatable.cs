namespace Routing
{
    public interface ICorrelatable<THashKey>
    {
        THashKey CorrelationId { get; }
    }
}