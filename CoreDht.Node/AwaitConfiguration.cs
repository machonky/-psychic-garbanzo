namespace CoreDht.Node
{
    public class AwaitConfiguration
    {
        public const int AwaitDefault = 200;
        public const int AckDefault = 50;
        public const int NetworkQueryDefault = 75;

        public int InitTimeout { get; set; }
        public int AwaitTimeout { get; set; }
        public int AckTimeout { get; set; }
        public int NetworkQueryTimeout { get; set; }

        public AwaitConfiguration()
        {
            AwaitTimeout
#if DEBUG
                = int.MaxValue;
#else
                = AwaitDefault;
#endif
            AckTimeout = AckDefault;
            NetworkQueryTimeout = NetworkQueryDefault;
            InitTimeout = 500;
        }
    }
}