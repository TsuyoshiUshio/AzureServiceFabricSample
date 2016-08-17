using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using SensorActor.Interfaces;
using System.Diagnostics;

namespace SensorAggregationTest
{
    class Program
    {
        static Random mRand = new Random();
        static void Main(string[] args)
        {
            SetIndexes();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            setTemperatures(100, 50);
            watch.Stop();
            Console.WriteLine("Time to set temperatures: " + watch.ElapsedMilliseconds);
            Console.ReadKey();

        }


        static void setTemperatures(double average, double variation)
        {
            Task[] tasks = new Task[1000];
            Parallel.For(0, 1000, i =>
            {
                var proxy = ActorProxy.Create<ISensorActor>(new ActorId(i),
                    "fabric:/SensorAggregationApplication");
                tasks[i] = proxy.SetTemperatureAsync(
                    average + (mRand.NextDouble() - 0.5) * 2 * variation);
            });
            Task.WaitAll(tasks);
        }

        static void SetIndexes()
        {
            Task[] tasks = new Task[1000];
            Parallel.For(0, 1000, i =>
            {
                var proxy = ActorProxy.Create<ISensorActor>(new ActorId(i),
                    "fabric:/SensorAggregationApplication");
                tasks[i] = proxy.SetIndexAsync(i);
            });
            Task.WaitAll(tasks);
        }
    }
}
