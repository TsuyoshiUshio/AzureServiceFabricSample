using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Microsoft.ServiceFabric.Services.Client;
using Common;

namespace SimpleStoreClient
{
    public class Client : ServicePartitionClient<WcfCommunicationClient<IShoppingCartService>>,
        IShoppingCartService
    {
        public Client(WcfCommunicationClientFactory<IShoppingCartService> clientFactory, Uri serviceName, string customerId)
           : base(clientFactory, serviceName, new ServicePartitionKey(customerId))
        {

        }           

        public Task AddItem(ShoppingCartItem item)
        {
            return this.InvokeWithRetryAsync(client => client.Channel.AddItem(item));
        }

        public Task DeleteItem(string productName)
        {
            return this.InvokeWithRetryAsync(client => client.Channel.DeleteItem(productName));
        }

        public Task<List<ShoppingCartItem>> GetItems()
        {
            return this.InvokeWithRetryAsync(client => client.Channel.GetItems());
        }
    }
}
