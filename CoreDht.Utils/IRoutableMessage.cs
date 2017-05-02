namespace CoreDht.Utils
{
    public interface IRoutableMessage<out THashKey>
    {
        THashKey RoutingTarget { get; }
    }
}