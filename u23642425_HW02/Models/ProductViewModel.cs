using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23642425_HW02.Models
{
    public class ProductViewModel
    {
        public string ProductName { get; set; }
        public string BrandName { get; set; }
        public string CategoryName { get; set; }
        public int ModelYear { get; set; }
        public decimal ListPrice { get; set; }
        public byte[] ImageData { get; set; }
        public int? Quantity { get; set; }
    }

}