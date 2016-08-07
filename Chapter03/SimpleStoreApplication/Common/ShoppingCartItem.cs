using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public struct ShoppingCartItem
    {
        public string ProductName { get; set; }
        public double UnitPrice { get; set; }
        public int Amount { get; set; }
        public double LineTotal
        {
            get
            {
                return Amount * UnitPrice;
            }
        }
    }
}
