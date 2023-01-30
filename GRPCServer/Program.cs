using System;
using System.Linq;
using Grpc.Core;
using HelloWorld;
using ZooKeeperNet;

namespace GRPCServer
{
    class GRPCServiceDiscoveryInfo
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }

        public override string ToString()
        {
            return $"{IPAddress}:{Port}";
        }

    }

    public static class GRPCServerExtension
    {
        public static void StartWithDiscovery(this Server grpcServer, string zooKeeperPath)
        {
            grpcServer.Ports.Add(new ServerPort("localhost", ServerPort.PickUnused, ServerCredentials.Insecure));
            grpcServer.Start();
           

            var chosenPost = grpcServer.Ports.First();

            var grpcServiceDiscoveryInfo = new GRPCServiceDiscoveryInfo
            {
                IPAddress = chosenPost.Host,
                Port = chosenPost.BoundPort
            };
            //convert the grpcServiceDiscoveryInfo in to byte array
            var grpcServiceDiscoveryInfoBytes = System.Text.Encoding.UTF8.GetBytes(grpcServiceDiscoveryInfo.ToString());

            using (var zooKeeper = new ZooKeeper("localhost:21811", TimeSpan.FromSeconds(100), null))
            {
                var path = "/IPS";
                //check if the node exist
                if (zooKeeper.Exists(path, false) == null)
                {
                    //create the node
                    zooKeeper.Create(path, grpcServiceDiscoveryInfoBytes, Ids.OPEN_ACL_UNSAFE, CreateMode.Persistent);
                }
                else
                {
                    //update the node
                    zooKeeper.SetData(path, grpcServiceDiscoveryInfoBytes, -1);
                }
            }

        }
    }
    public static class Program
    {
        public static void Main(string[] argv)
        {
            var server = new Server
            {
                Services = { Greeter.BindService(new GreeterImpl()) },
            };
            server.StartWithDiscovery("/IPS");

            System.Console.WriteLine("Server listening on port 50051");
            System.Console.WriteLine("Press any key to stop the server...");
            System.Console.ReadKey();
            server.ShutdownAsync().Wait();
        }
    }
}
