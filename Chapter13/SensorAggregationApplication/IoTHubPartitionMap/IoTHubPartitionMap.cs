using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using IoTHubPartitionMap.Interfaces;
using System.Runtime.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace IoTHubPartitionMap
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class IoTHubPartitionMap : Actor, IIoTHubPartitionMap
    {

        IActorTimer mTimer;
        [DataContract]
        internal sealed class ActorState
        {
            [DataMember]
            public List<string> PartitionNames { get; set; }
            [DataMember]
            public Dictionary<string, DateTime> PartitionLeases { get; set; }

            //private IActorStateManager stateManager;
            //internal static ActorState GetStateAsync(IActorStateManager stateManager)
            //{
            //    var state = stateManager.GetStateAsync<ActorState>("MyState").Result;
            //    state.stateManager = stateManager;
            //    return state;
            //}

            //internal static ActorState GetState(IActorStateManager stateManager)
            //{
            //    var state = stateManager.GetStateAsync<ActorState>("MyState").GetAwaiter().GetResult();
            //    state.stateManager = stateManager;
            //    return state;
            //}
            //internal void Save()
            //{
            //    stateManager.AddOrUpdateStateAsync<ActorState>("MySate", this, (k, v) => this).GetAwaiter();
            //}
        }

        public async Task<string>  LeaseTHubPartitionAsync()
        {
            string ret = "";
            var state = await this.StateManager.GetStateAsync<ActorState>("MyState");
            foreach (string partition in state.PartitionNames)
            {
                ActorEventSource.Current.ActorMessage(this, "********** LeaseTHub: {0}******", partition);
                if (!state.PartitionLeases.ContainsKey(partition))
                {
                    ActorEventSource.Current.ActorMessage(this, "********** LeaseTHub added: {0}******", partition);
                    state.PartitionLeases.Add(partition, DateTime.Now);
                    ret = partition;
                    break;
                }
            }
            ActorEventSource.Current.ActorMessage(this, "********** LeaseTHub ret: {0}******", ret);
            await this.StateManager.SetStateAsync<ActorState>("MyState", state);
            await this.SaveStateAsync();
            return ret;
        }

        public async Task<string> RenewIoTHubPartitionLeaseAsync(string partition)
        {
            string ret = "";
            var state = await this.StateManager.GetStateAsync<ActorState>("MyState");
            
            if (state.PartitionLeases.ContainsKey(partition))
            {
                state.PartitionLeases[partition] = DateTime.Now;
                await this.StateManager.SetStateAsync<ActorState>("MyState", state);
                ret = partition;
            }
            return ret;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            var state = await this.StateManager.TryGetStateAsync<ActorState>("MyState");
            if (!state.HasValue)
            {
                var actorState = new ActorState
                {
                    PartitionNames = new List<string>(),
                    PartitionLeases = new Dictionary<string, DateTime>()
                };
                await this.StateManager.TryAddStateAsync<ActorState>("MyState", actorState);

                await ResetPartitionNamesAsync();
                mTimer = RegisterTimer(CheckLease, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
                await this.SaveStateAsync();
            }
        }

        protected override Task OnDeactivateAsync()
        {

            if (mTimer != null)
                UnregisterTimer(mTimer);
            return base.OnDeactivateAsync();


        }

        private async Task ResetPartitionNamesAsync()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString("HostName=iote2e.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Bsp4+D5at3lTacsNaZPvx0FhVvrdDa8LGFzKS/B6zzQ=", "messages/events");
            var partitions = eventHubClient.GetRuntimeInformation().PartitionIds;
            var state  =  await this.StateManager.GetStateAsync<ActorState>("MyState");
            ActorEventSource.Current.ActorMessage(this, "********** Partition initializer ******");
            foreach (string partition in partitions)
            {
                ActorEventSource.Current.ActorMessage(this, "**********  Partition ={0} ******", partition);
                state.PartitionNames.Add(partition);
            }
            await this.StateManager.SetStateAsync<ActorState>("MyState", state);
            await this.SaveStateAsync();
        }
        private async Task CheckLease(Object state)
        {
            var stateProxy = await this.StateManager.GetStateAsync<ActorState>("MyState");
            List<string> keys = stateProxy.PartitionLeases.Keys.ToList();
            foreach (string key in keys)
            {
                if (DateTime.Now - stateProxy.PartitionLeases[key] >= TimeSpan.FromSeconds(60))
                    stateProxy.PartitionLeases.Remove(key);

            }
            await this.StateManager.SetStateAsync<ActorState>("MyState", stateProxy);
            await this.SaveStateAsync();
        }

    }


}
