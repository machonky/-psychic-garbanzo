namespace Routing
{
    public interface ICorrelatedMessage<THashKey>
    {
        THashKey CorrelationId { get; }
    }
}