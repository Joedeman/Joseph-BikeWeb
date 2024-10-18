using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace u23642425_HW02.Models
{
    public class OrderConfirmationView
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }  // New property for customer name
        public string Surnamee { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItemViewModel> OrderItems { get; set; }
        public StoreViewModel StoreDetails { get; set; }  // New property for store details
    }

    public class StoreViewModel  // New ViewModel for store details
    {
        public string StoreName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }


    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal ListPrice { get; set; }
        public decimal Discount { get; set; }
    }
}
