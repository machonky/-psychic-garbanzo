using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CoreDht;
using CoreDht.Utils;
using CoreDht.Utils.Hashing;
using CoreDht.Node;

namespace NetworkRouting
{

    class Program
    {
        void Run(string[] args)
        {
            using (var janitor = new DisposableStack())
            {
                var logger = janitor.Push(new FileLogger());

                var hostEntry = _dnsProvider.GetHostEntry("localhost");
                var config = new MyAppNodeConfiguration
                {
                    HashingService = _hashingService,
                    Serializer = _msgSerializer,
                    NodeSocketFactory = _nodeSocketFactory,
                    Clock = _clock,
                    CorrelationFactory = _correlationFactory,
                    SuccessorCount = 3,
                    SeedNode = $"{hostEntry.HostName}:{SeedPort}",
                    LoggerDelegate = logger.WriteLine,
                    ExpiryCalculator = _expiryCalculator,
                    Random = _random,
                    ActionTimerFactory = _actionTimerFactory,
                    MarshallerFactory = _marshallerFactory,
                    CommunicationManagerFactory = _communicationManagerFactory,
                };

                var factory = new MyAppNodeFactory(config);

                var hostAndPort0 = $"{hostEntry.HostName}:{SeedPort}";
                var id0 = Node.CreateIdentifier(hostAndPort0);

                var nodes = new List<Node>();

                nodes.Add(janitor.Push(factory.CreateNode(id0, hostAndPort0)));

                for (int i = 9001; i < 9002; ++i)
                {
                    var hostAndPort = $"{hostEntry.HostName}:{i}";
                    var id = Node.CreateIdentifier(hostAndPort);
                    nodes.Add(janitor.Push(factory.CreateNode(id, hostAndPort)));
                    Thread.Sleep(500);
                }

                Console.WriteLine();
                //foreach (var node in nodes)
                //{
                //    Console.WriteLine($"{node.Identity}\tS: {node.Successor}\tP: {node.Predecessor}");
                //}
                Console.ReadKey();
                //Console.ReadKey();

                //var terminate = new TerminateNode(node0.Identity.RoutingHash);
                //node0.Publish(terminate);

                //terminate = new TerminateNode(node1.Identity.RoutingHash);
                //node1.Publish(terminate);

                //terminate = new TerminateNode(node2.Identity.RoutingHash);
                //node2.Publish(terminate);
            }

            Console.ReadKey();
        }

        private readonly IConsistentHashingService _hashingService;
        private readonly IMessageSerializer _msgSerializer;
        private readonly IDnsProvider _dnsProvider;
        private readonly INodeSocketFactory _nodeSocketFactory;
        private readonly IUtcClock _clock;
        private readonly ICorrelationFactory<CorrelationId> _correlationFactory;
        private readonly IActionTimerFactory _actionTimerFactory;
        private readonly IExpiryTimeCalculator _expiryCalculator;
        private readonly IRandomNumberGenerator _random;
        private readonly INodeMarshallerFactory _marshallerFactory;
        private readonly ICommunicationManagerFactory _communicationManagerFactory;
        private const int SeedPort = 9000;

        Program()
        {
            _clock = new UtcClock();
            _dnsProvider = new DnsProvider();
            _hashingService = new Md5HashingService();
            _msgSerializer = new MessageSerializer();
            _nodeSocketFactory = new InProcNodeSocketFactory();
            _correlationFactory = new CorrelationIdFactory();
            _actionTimerFactory = new ActionTimerFactory();
            _expiryCalculator = new ExpiryTimeCalculator(_clock);
            _random = new RandomNumberGenerator(_correlationFactory);
            _marshallerFactory = new NodeMarshallerFactory(_msgSerializer);
            _communicationManagerFactory = new CommunicationManagerFactory();
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }

    public class FileLogger : IDisposable
    {
        public FileLogger()
        {
            _stream = new FileStream("output.txt",FileMode.Create, FileAccess.ReadWrite);
            _writer = new StreamWriter(_stream);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls
        private readonly FileStream _stream;
        private readonly StreamWriter _writer;

        public void WriteLine(string value)
        {
            _writer.WriteLine(value);
            Console.WriteLine(value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _writer.Dispose();
                    _stream.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}

