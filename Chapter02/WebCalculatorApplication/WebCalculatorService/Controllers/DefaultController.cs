using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCalculatorService.Controllers
{
    using System.Collections.Generic;
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
