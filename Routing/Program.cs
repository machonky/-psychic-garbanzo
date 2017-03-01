using System;
using System.Collections.Generic;
using Routing.Messages;


namespace Routing
{
    class Program
    {
        public void Run(string[] args) 
        {
            var AliceNode = new ChordNode("Alice");
            var BobNode = new ChordNode("Bob");
            var CharlesNode = new ChordNode("Charles");

            var msg = BobNode.EmitJoinNetwork(AliceNode.NodeKey);
            AliceNode.TryPublish(msg);

            var AliceHash = AliceNode.NodeKey;

            BobNode.TryPublish(new StartUsefulWork(AliceHash) { Owner = "Alice" });
            CharlesNode.TryPublish(new DoUsefulWork(AliceHash) { Data = "Hello world" });

            Console.ReadKey();
        }

        static void Main(string[] args)
        {
            var theApp = new Program();
            theApp.Run(args);
        }
    }
}
