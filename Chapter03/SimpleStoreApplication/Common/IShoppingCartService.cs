using System.Collections.Generic;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IShoppingCartService
    {
        [OperationContract]
        Task AddItem(ShoppingCartItem item);
        [OperationContract]
        Task DeleteItem(string productName);
        [OperationContract]
        Task<List<ShoppingCartItem>> GetItems();
    }
}
