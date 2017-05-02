using System;
using System.Linq;
using CoreDht;
using CoreDht.Node;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace NetworkRouting
{
    public class MyAppNode : Node
    {
        private MyAppDataRepo _appRepo;
        private ApplicationHandler _appHandler;

        public MyAppNode(NodeInfo identity, MyAppNodeConfiguration config) : 
            base(identity, config)
        {
            var factory = new MyAppDataFactory(config.LoggerDelegate);
            _appRepo = new MyAppDataRepo(factory.CreateMyAppData);
            _appHandler = new ApplicationHandler(this);

            MessageBus.Subscribe(_appHandler);
            MessageBus.Subscribe(_appRepo);
        }

        public class ApplicationHandler 
            //: IHandle<NodeInitialised>
        {
            private readonly MyAppNode Node;

            public ApplicationHandler(MyAppNode node)
            {
                Node = node;
            }

            //public void Handle(NodeInitialised message)
            //{
            //    Node.Logger?.Invoke($"{Node.Identity.Identifier} initialised");
            //}
        }

        //protected override RequestKeys.Reply CreateRequestKeysReply(RequestKeys message, Guid receiptCorrelation)
        //{
        //    var startAt = message.StartAt;
        //    var successorId = message.Successor.RoutingHash;

        //    Logger?.Invoke($"Node:{Identity} Collecting keys from id:{startAt} to id:{successorId} for transport to {message.Successor}");

        //    var result =
        //        from repoEntry in _appRepo
        //        where repoEntry.Key >= startAt || repoEntry.Key <= successorId
        //        select KeyValueDto.New(repoEntry.Key, repoEntry.Value);

        //    Logger?.Invoke($"Found {result.Count()} entries.");

        //    return new MyAppRequestKeysReply(message.Identity, message.CorrelationId, receiptCorrelation, result.ToArray());
        //}
    }

    //public class MyAppRequestKeysReply : RequestKeys.Reply
    //{
    //    public KeyValueDto<MyAppData>[] EvictedKeys { get; }

    //    public MyAppRequestKeysReply(NodeInfo identity, Guid correlationId, Guid receiptCorrelation, KeyValueDto<MyAppData>[] evictedKeys) 
    //        : base(identity, correlationId, receiptCorrelation)
    //    {
    //        EvictedKeys = evictedKeys;
    //    }
    //}
    
    public class MyAppDataRepo : RoutableRepository<ConsistentHash, MyAppData>
    {
        public MyAppDataRepo(Func<Message, MyAppData> factory):base(factory)
        { }
    }

    public class MyAppDataFactory
    {
        public Action<string> Logger { get; }

        public MyAppDataFactory(Action<string> logger)
        {
            Logger = logger;
        }

        public MyAppData CreateMyAppData(Message msg)
        {
            var routableMsg = (RoutableMessage) msg; 
            return new MyAppData(routableMsg.RoutingTarget, Logger);
        }
    }

    public class MyAppData : RepositoryItem<ConsistentHash>
        //, IAmTriggeredBy<MyInitialAppMessage>
    {
        public Action<string> Logger { get; }

        public MyAppData(ConsistentHash correlationId, Action<string> logger) : base(correlationId)
        {
            Logger = logger;
            Logger?.Invoke($"Instantiating MyAppData handler for {correlationId}");
        }

        //public void Handle(MyInitialAppMessage message)
        //{
        //    Logger?.Invoke($"Completeing intialisation for {CorrelationId}");
        //}
    }

    //public class MyInitialAppMessage : RoutableMessage
    //{
    //    public MyInitialAppMessage(ConsistentHash routingTarget) : base(routingTarget)
    //    {}

    //    public string Data { get; set; }
    //}
}
