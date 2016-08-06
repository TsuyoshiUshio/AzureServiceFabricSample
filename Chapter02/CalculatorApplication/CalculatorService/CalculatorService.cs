using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using System.ServiceModel;
namespace CalculatorService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class CalculatorService : StatelessService, ICalculatorService 
    {
        public CalculatorService(StatelessServiceContext context)
            : base(context)
        { }

        public Task<string> Add(int a, int b)
        {
            return Task.FromResult<string>(string.Format("Instance {0} returns: {1}",
                this.Context.InstanceId,
                a + b));
        }

        public Task<string> Subtract(int a, int b)
        {
            return Task.FromResult<string>(string.Format("Instance {0} returns: {1}",
                this.Context.InstanceId,
                a - b));
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener(context =>
                    new WcfCommunicationListener<ICalculatorService>(
                        wcfServiceObject:this,
                        serviceContext:context,
                        endpointResourceName: "ServiceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    )
            )};
        }

        private NetTcpBinding CreateListenBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxConnections = int.MaxValue,
                MaxReceivedMessageSize = 1024 * 1024
            };
            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            return binding;
        }

    }
}
