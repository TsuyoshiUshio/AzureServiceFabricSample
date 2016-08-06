using System;
using CalculatorService;
using Microsoft.ServiceFabric.Services.Client;
using System.Fabric;
using System.ServiceModel;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;

namespace CalculatorClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri ServiceName = new Uri("fabric:/CalculatorApplication/CalculatorService");
            ServicePartitionResolver serviceResolver = new ServicePartitionResolver(() =>
            new FabricClient());
            NetTcpBinding binding = CreateClientConnectionBinding();
            Client calcClient = new Client(new WcfCommunicationClientFactory<ICalculatorService>(servicePartitionResolver: serviceResolver, clientBinding: binding), ServiceName);
            Console.WriteLine(calcClient.Add(3, 5).Result);
            Console.ReadKey();
        }
        private static NetTcpBinding CreateClientConnectionBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxReceivedMessageSize = 1024 * 1024
            };
            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;
            return binding;
        }
       
    }
}
