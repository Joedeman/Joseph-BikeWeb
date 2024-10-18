using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23642425_HW02.Models
{
    public class Products
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int BrandID { get; set; }
        public int CategoryID { get; set; }
        public short ModelYear { get; set; }
        public decimal ListPrice { get; set; }
        public byte[] ImageData { get; set; }

        public string ImageBase64
        {
            get
            {
                return "data:image/jpeg;base64," + Convert.ToBase64String(ImageData);
            }
        }
    }


}