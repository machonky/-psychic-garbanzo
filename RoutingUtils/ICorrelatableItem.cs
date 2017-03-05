namespace CoreDht
{
    public interface ICorrelatableItem<T>
    {
        T CorrelationId { get; }
    }
}