namespace CoreDht
{
    public interface ICorrelationFactory<out T>
    {
        T GetNextCorrelation();
    }
}