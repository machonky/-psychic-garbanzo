namespace Routing
{
    public interface IRoutableMessage<THashKey>
    {
        THashKey RoutingTarget { get; }
    }
}