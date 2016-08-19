using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using IoTHubPartitionMap.Interfaces;
using Microsoft.ServiceBus.Messaging;
using System.Text;

namespace SensorDataProcessor
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class SensorDataProcessor : StatefulService
    {
        public SensorDataProcessor(StatefulServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see http://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new ServiceReplicaListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
   
                ServiceEventSource.Current.ServiceMessage(this, "********** Run Async! ******");
            DateTime timeStamp = DateTime.Now;
            var proxy = ActorProxy.Create<IIoTHubPartitionMap>(new ActorId(1) ,
                    "fabric:/SensorAggregationApplication");
                var eventHubClient = EventHubClient.CreateFromConnectionString("HostName=iote2e.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Bsp4+D5at3lTacsNaZPvx0FhVvrdDa8LGFzKS/B6zzQ=", "messages/events");

                while (!cancellationToken.IsCancellationRequested)
                {
                    string partition = await proxy.LeaseTHubPartitionAsync();

                if (partition == "")
                    {
                        ServiceEventSource.Current.ServiceMessage(this, "********** Partition = '' ******");
                        await Task.Delay(TimeSpan.FromSeconds(15), cancellationToken);
                    }
                    else
                    {
                        ServiceEventSource.Current.ServiceMessage(this, "********** coming! Partition ={0} ******", partition);
                        var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver("3", DateTime.UtcNow);
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            EventData eventData = await eventHubReceiver.ReceiveAsync();

                        if (eventData != null)
                        {
                            ServiceEventSource.Current.ServiceMessage(this, "********** the event data is coming! ******");
                            string data = Encoding.UTF8.GetString(eventData.GetBytes());
                            ServiceEventSource.Current.ServiceMessage(this, "Message: {0}", data);
                            timeStamp = DateTime.Now;
                        }
                        else
                        {
                            if (DateTime.Now - timeStamp > TimeSpan.FromSeconds(20))
                            {
                                ServiceEventSource.Current.ServiceMessage(this, "********** not yet! ******");
                                  string lease = await proxy.RenewIoTHubPartitionLeaseAsync(partition);
                                  ServiceEventSource.Current.ServiceMessage(this, "********** lease {0} ******", lease);
                                if (lease == "")
                                    break;
                            }
                        }
                        }


                    }
                }
 
        }
    }
}
