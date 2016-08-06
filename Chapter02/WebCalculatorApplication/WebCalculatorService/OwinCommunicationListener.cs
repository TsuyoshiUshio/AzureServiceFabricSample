using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using System.Fabric.Description;
using Microsoft.Owin.Hosting;
using System.Fabric;
namespace WebCalculatorService
{
    public class OwinCommunicationListener : ICommunicationListener
    {
        private readonly IOwinAppBuilder startup;
        private readonly ServiceContext serviceContext;  
        private IDisposable serverHandle;
        private string listeningAddress;
        private string appRoot;

        public OwinCommunicationListener(IOwinAppBuilder startup, ServiceContext serviceContext, string appRoot)
        {
            this.startup = startup;
            this.serviceContext = serviceContext;
            this.appRoot = appRoot;
        }
        public void Abort()
        {
            this.StopWebServer();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            this.StopWebServer();
            return Task.FromResult(true);   
        }
        private void StopWebServer()
        {
            if (this.serverHandle != null)
            {
                try
                {
                    this.serverHandle.Dispose();
                }
                catch (ObjectDisposedException ex)
                {
                    this.StopWebServer();
                    throw;
                }
            }
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            EndpointResourceDescription serviceEndpoint = this.serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint");
            int port = serviceEndpoint.Port;
            this.listeningAddress = String.Format("http://+:{0}/{1}/", port, appRoot);

            this.serverHandle = WebApp.Start(this.listeningAddress,
                appBuilder => this.startup.Configuration(appBuilder));
            string resultAddress = this.listeningAddress.Replace("+",
                FabricRuntime.GetNodeContext().IPAddressOrFQDN);
            ServiceEventSource.Current.Message("Listening on {0}", resultAddress);
            return Task.FromResult(resultAddress);
        }
    }
}
