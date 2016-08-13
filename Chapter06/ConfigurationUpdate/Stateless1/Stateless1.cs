using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Stateless1
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Stateless1 : StatelessService
    {


        public Stateless1(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            int incrementStep = 1;



            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var configSection = this.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                var text = configSection.Settings.Sections["MyConfigSection"]
                    .Parameters["IncrementStep"].Value;
                incrementStep = int.Parse(text);

                ServiceEventSource.Current.ServiceMessage(this, "Working-{0}", iterations += incrementStep);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
