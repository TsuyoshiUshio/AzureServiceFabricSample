using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Data;
using SensorActor.Interfaces;
using System.ComponentModel;
using FloorActor.Interfaces;

namespace SensorActor
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
    internal class SensorActor : Actor, ISensorActor
    {
        [DataContract]
        public sealed class ActorState
        {
            [DataMember]
            public double Temperature {
                get; set;
            }
            [DataMember]
            public int Index { get; set; }
        }


        [ReadOnly(true)]
        public async Task<int> GetIndexAsync()
        {
            var state = await this.StateManager.TryGetStateAsync<ActorState>("MyState");
            return state.Value.Index;
        }

        [ReadOnly(true)]
        public async Task<double> GetTemperatureAsync()
        {
            var state = await this.StateManager.TryGetStateAsync<ActorState>("MyState");
            return state.Value.Temperature;
       }

        public async Task SetIndexAsync(int index)
        {
            var stateValue = await this.StateManager.TryGetStateAsync<ActorState>("MyState");
            var state = stateValue.Value;
            state.Index = index;
            await this.StateManager.SetStateAsync<ActorState>("MyState", state);
        }

        public async Task SetTemperatureAsync(double temperature)
        {
            var stateValue = await this.StateManager.TryGetStateAsync<ActorState>("MyState");
            var state = stateValue.Value;
            state.Temperature = temperature;
            await this.StateManager.SetStateAsync<ActorState>("MyState", state);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "SensorActor activated.");
            var state = await this.StateManager.TryGetStateAsync<ActorState>("MyState");

            if (!state.HasValue)
            {
                var actorState = new ActorState() { Temperature = 0 };
                await this.StateManager.SetStateAsync<ActorState>("MyState", actorState);
            }
        }

    }
}
