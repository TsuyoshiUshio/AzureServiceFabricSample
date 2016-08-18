using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace DeviceRegistry
{
    class Program
    {
        static void Main(string[] args)
        {
            

            RegistryManager registryManager = RegistryManager.CreateFromConnectionString("HostName=iote2e.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=Bsp4+D5at3lTacsNaZPvx0FhVvrdDa8LGFzKS/B6zzQ=");
            string deviceId = "DemoDevice";
            var device = registryManager.GetDeviceAsync(deviceId).Result;
            if (device == null)
            {
                device = registryManager.AddDeviceAsync(new Device(deviceId)).Result;
            }
            Console.WriteLine("Device Key: " + device.Authentication.SymmetricKey.PrimaryKey);
            Console.ReadKey();
        }
    }
}
