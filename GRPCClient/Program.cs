using System;
using Grpc.Core;
using HelloWorld;
using ZooKeeperNet;

class GrpcServiceDiscoveryInfo
{
    public string IPAddress { get; set; }
    public int Port { get; set; }

    public override string ToString()
    {
        return $"{IPAddress}:{Port}";
    }
}

namespace GRPCClient
{
    class Program
    {
        static void Main(string[] args)
        {
            GrpcServiceDiscoveryInfo grpcServiceDiscoveryInfo;
            using (var zooKeeper = new ZooKeeper("localhost:21811", TimeSpan.FromSeconds(5), null))
            {
                // Perform operations with ZooKeeper
                var grpcServiceDiscoveryInfoBytes = zooKeeper.GetData("/IPS", false, null);

                //parse the byte array to GRPCServiceDiscoveryInfo
                var grpcServiceDiscoveryInfoText = System.Text.Encoding.UTF8.GetString(grpcServiceDiscoveryInfoBytes);
                var grpcServiceDiscoveryInfoParts = grpcServiceDiscoveryInfoText.Split(':');
                grpcServiceDiscoveryInfo = new GrpcServiceDiscoveryInfo
                {
                    IPAddress = grpcServiceDiscoveryInfoParts[0],
                    Port = int.Parse(grpcServiceDiscoveryInfoParts[1])
                };
            }
            
            Channel channel = new Channel($"{grpcServiceDiscoveryInfo.IPAddress}:{grpcServiceDiscoveryInfo.Port}", ChannelCredentials.Insecure);
            var client = new Greeter.GreeterClient(channel);

            String user = "you";
            var reply = client.SayHello(new HelloRequest { Name = user });
            Console.WriteLine("Greeting: " + reply.Message);

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}