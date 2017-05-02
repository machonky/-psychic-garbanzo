using System;

namespace CoreDht.Utils
{
    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        private readonly Random _rnd;

        public RandomNumberGenerator(ICorrelationFactory<CorrelationId> seedFactory)
        {
            _rnd = new Random(seedFactory.GetNextCorrelation().GetHashCode());
        }

        public int Next()
        {
            return _rnd.Next();
        }

        public int Next(int minValue, int maxValue)
        {
            return _rnd.Next(minValue, maxValue);
        }
    }
}