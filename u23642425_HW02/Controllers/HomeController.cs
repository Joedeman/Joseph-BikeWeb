using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using u23642425_HW02.Models;

namespace u23642425_HW02.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
           
            return View();
        }



        public BikeStoresDbContext _dbContext = new BikeStoresDbContext();
        public ActionResult Buyer(string storeLocation, DateTime? orderDate)
        {
            // Fetch all orders that include customers and stores, and filter by customertype_id (2 or 3)
            var buyersQuery = _dbContext.orders
                                        .Include(o => o.store)
                                        .Include(o => o.customer)
                                        .Where(o => o.customer.customertype_id == 2 || o.customer.customertype_id == 3)
                                        .AsQueryable();

            // Apply filtering by store location if provided
            if (!string.IsNullOrEmpty(storeLocation))
            {
                buyersQuery = buyersQuery.Where(o => o.store.store_name == storeLocation);
            }

            // Apply filtering by order date if provided
            if (orderDate.HasValue)
            {
                var orderDateOnly = orderDate.Value.Date; // Ensure we're comparing dates only
                buyersQuery = buyersQuery.Where(o => o.order_date == orderDateOnly);
            }

            // Execute the query and get the filtered results
            var buyers = buyersQuery.ToList();

            return View(buyers);
        }




        public ActionResult Seller(string storeLocation, DateTime? orderDate)
        {
            // Fetch all orders that include customers, stores, and order items
            var sellerQuery = _dbContext.orders
                                        .Include(o => o.store)
                                        .Include(o => o.customer)
                                        .Include(o => o.staff)
                                        .Include(o => o.order_items.Select(oi => oi.product)) // Include order_items and related products
                                        .Where(o => o.customer.customertype_id == 1 || o.customer.customertype_id == 3)
                                        .AsQueryable();

            // Apply filtering by store location if provided
            if (!string.IsNullOrEmpty(storeLocation))
            {
                sellerQuery = sellerQuery.Where(o => o.store.store_name == storeLocation);

                // If the store location is "Rowlett Bikes", fetch product info from order_items
                if (storeLocation == "Rowlett Bikes")
                {
                    // This ensures that the products linked via order_items are included
                    sellerQuery = sellerQuery.Where(o => o.order_items.Any());
                }
            }

            // Apply filtering by order date if provided
            if (orderDate.HasValue)
            {
                var orderDateOnly = orderDate.Value.Date; 
                sellerQuery = sellerQuery.Where(o => o.order_date == orderDateOnly);
            }

            // Execute the query and get the filtered results
            var sellers = sellerQuery.ToList();

            return View(sellers);
        }

    }
}