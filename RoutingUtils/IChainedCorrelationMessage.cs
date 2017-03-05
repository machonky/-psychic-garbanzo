namespace CoreDht
{
    public interface IChainedCorrelationMessage<out T>
    {
        T Head { get; }
        T Tail { get; }
    }
}