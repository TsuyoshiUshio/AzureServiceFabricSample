using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace SimulatedGateway
{
    class Program
    {
        static void Main(string[] args)
        {
            string iotHostName = "iote2e.azure-devices.net";
            string deviceId = "DemoDevice";
            string deviceKey = "9mR5ehuHw77iRCGVx0cNPmIzY6hz1u3U05N4sSOz3y8=";
            Random rand = new Random();
            var deviceClient = DeviceClient.Create(iotHostName, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey));
            while(true)
            {
                double temperature = rand.NextDouble() * 100;
                var temperatureData = new
                {
                    deviceId = deviceId,
                    temperature = temperature
                };

                var message = new Message(Encoding.ASCII.GetBytes
                    (JsonConvert.SerializeObject(temperatureData)));
                deviceClient.SendEventAsync(message).Wait();
                Console.WriteLine(".");
                Thread.Sleep(1000);
            }
        }
    }
}
