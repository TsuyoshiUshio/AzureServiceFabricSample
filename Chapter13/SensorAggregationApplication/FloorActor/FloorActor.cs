﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using FloorActor.Interfaces;
using System.Runtime.Serialization;
using System.ComponentModel;
using SensorActor.Interfaces;
namespace FloorActor
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
    internal class FloorActor : Actor, IFloorActor
    {
     [DataContract]
     public sealed class ActorState
        {
            [DataMember]
            public double Temperature { get; set; }
        }  

        [ReadOnly(true)]
        public Task<double> GetTemperatureAsync()
        {
            Task<double>[] tasks = new Task<double>[1000];
            double[] readings = new double[1000];
            Parallel.For(0, 1000, i =>
            {
                var proxy = ActorProxy.Create<ISensorActor>
                (new ActorId(i), "fabric:/SensorAggregationApplication");
                tasks[i] = proxy.GetTemperatureAsync();
            });
            Task.WaitAll(tasks);
            Parallel.For(0, 1000, i =>
            {
                readings[i] = tasks[i].Result;
            });
            return Task.FromResult(readings.Average());
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "FloorActor activated.");
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
