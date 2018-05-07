using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicFarmStore
{
    public class SampleService
    {
        public decimal CalculateSalesTax(decimal price)
        {
            return price * 0.1m;
        }
    }
}
