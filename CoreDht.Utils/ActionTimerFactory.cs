namespace CoreDht.Utils
{
    public class ActionTimerFactory : IActionTimerFactory
    {
        public IActionTimer Create()
        {
            return new ActionTimer();
        }
    }
}