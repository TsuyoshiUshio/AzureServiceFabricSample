using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CalculatorService
{
    public interface ICalculatorService : IService
    {
        Task<int> Add(int a, int b);
        Task<int> Subtract(int a, int b);
    }
}
