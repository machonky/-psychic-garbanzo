namespace CoreDht
{
    public interface ICorrelatedMessage<out T>
    {
        T CorrelationId { get; }
    }
}