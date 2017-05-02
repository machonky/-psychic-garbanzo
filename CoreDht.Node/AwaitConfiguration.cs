namespace CoreDht.Node
{
    public class AwaitConfiguration
    {
        public const int AwaitDefault = 100;
        public const int AckDefault = 50;

        public int InitTimeout { get; set; }
        public int AwaitTimeout { get; set; }
        public int AckTimeout { get; set; }

        public AwaitConfiguration()
        {
            AwaitTimeout = AwaitDefault;
            AckTimeout = AckDefault;
            InitTimeout = 500;
        }
    }
}