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

        private ActorState StateProxy
        {
            get
            {
                return this.StateManager.TryGetStateAsync<ActorState>("MyState").GetAwaiter().GetResult().Value;
            }

            set
            {
                this.StateManager.AddOrUpdateStateAsync<ActorState>("MyState", value, (k, v) => value);
            }
        }

        [ReadOnly(true)]
        public Task<int> GetIndexAsync()
        {
            return Task.FromResult<int>(this.StateProxy.Index);
        }

        [ReadOnly(true)]
        public Task<double> GetTemperatureAsync()
        {
            return Task.FromResult<double>(this.StateProxy.Temperature);
        }

        public Task SetIndexAsync(int index)
        {
            Func<ActorState, int, ActorState> addIndex = (state, idx) => { state.Index = idx; return state; };
            addIndex(StateProxy, index);
            return Task.FromResult(true);
        }

        public Task SetTemperatureAsync(double temperature)
        {
            Func<ActorState, double, ActorState> addTemperture = (state, temp) => { state.Temperature = temp; return state; };
            addTemperture(StateProxy, temperature);
            return Task.FromResult(true);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "SensorActor activated.");
            var state = this.StateManager.TryGetStateAsync<ActorState>("MyState").GetAwaiter().GetResult().Value;

            if (state == null)
            {
                state = new ActorState() { Temperature = 0 };
            }
            this.StateManager.SetStateAsync<ActorState>("MyState", state);
            return Task.FromResult(true);
        }

    }
}
