
namespace WebCalculatorService.Controllers
{
    using System.Web.Http;
    public class DefaultController : ApiController
    {
        [HttpGet]
        public int Add(int a, int b)
        {
            return a + b;
        }

        [HttpGet]
        public int Subtract(int a, int b)
        {
            return a - b;
        }
    }
}
