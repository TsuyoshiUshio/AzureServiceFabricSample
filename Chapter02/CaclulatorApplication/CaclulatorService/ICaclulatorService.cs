using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaclulatorService
{
    interface ICaclulatorService: IService
    {
        Task<int> Add(int a, int b);
        Task<int> Subtract(int a, int b);
    }
}
