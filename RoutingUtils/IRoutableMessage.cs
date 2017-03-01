namespace CoreDht
{
    public interface IRoutableMessage<out THashKey>
    {
        THashKey RoutingTarget { get; }
    }
}