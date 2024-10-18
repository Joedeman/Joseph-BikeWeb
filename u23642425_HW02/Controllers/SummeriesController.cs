using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using u23642425_HW02.Models;

namespace u23642425_HW02.Controllers
{
    public class SummeriesController : Controller
    {
        // GET: Summeries
        public BikeStoresDbContext _dbContext = new BikeStoresDbContext();
        public ActionResult NewStocks()
        {
            
            int newStocks = 86;

            return View(newStocks); 
        }

        public ActionResult ListedForSale()
        {
            // Count all products in the database
            int listedForSale = _dbContext.products.Count();

            return View(listedForSale);  
        }

        public ActionResult TotalSold()
        {
            // Sum the quantity from the OrderItem (or SalesLines) table
            int totalSold = _dbContext.order_items
                .Sum(oi => oi.quantity);

            return View(totalSold);  // Pass the total quantity to the view
        }

        public ActionResult SalesPerBrand()
        {
            // Join OrderItem with Product and Brand tables to get the brand names and sum quantities
            var salesPerBrand = (from oi in _dbContext.order_items
                                 join p in _dbContext.products on oi.product_id equals p.product_id
                                 join b in _dbContext.brands on p.brand_id equals b.brand_id
                                 group oi by b.brand_name into brandGroup
                                 select new BrandSalesViewModel
                                 {
                                     Brand = brandGroup.Key,
                                     TotalSales = brandGroup.Sum(oi => oi.quantity)
                                 }).ToList();

            return View(salesPerBrand);
        }

        public ActionResult ListingsPerBrand()
        {
            // Query to get the count of bicycles available for sale (i.e., listed in products table) per brand
            var listingsPerBrand = (from p in _dbContext.products
                                    join b in _dbContext.brands on p.brand_id equals b.brand_id
                                    group p by b.brand_name into brandGroup
                                    select new BrandSalesViewModel
                                    {
                                        Brand = brandGroup.Key,
                                        TotalSales = brandGroup.Count() // Count the total number of bicycles listed per brand
                                    }).ToList();

            return View(listingsPerBrand);
        }


        public ActionResult AverageSalePerBrand()
        {
            // Query to calculate the average price of sold bicycles per brand
            var averageSalePerBrand = (from oi in _dbContext.order_items // Order_Items table for sold bicycles
                                       join p in _dbContext.products on oi.product_id equals p.product_id
                                       join b in _dbContext.brands on p.brand_id equals b.brand_id
                                       group new { oi, p } by b.brand_name into brandGroup
                                       select new BrandsAverageSale
                                       {
                                           Brand = brandGroup.Key,
                                           AverageSale = (decimal)brandGroup.Average(x =>
                                               x.oi.list_price * (1 - (x.oi.discount / 100))
                                           ) // Closing parenthesis for Average method
                                       }).ToList();

            return View(averageSalePerBrand);
        }

        public ActionResult TotalsPerBrandCategory()
        {
            // Query to count bicycles per brand and category
            var totalsPerBrandCategory = (from p in _dbContext.products 
                                          join b in _dbContext.brands on p.brand_id equals b.brand_id
                                          join c in _dbContext.categories on p.category_id equals c.category_id 
                                          group p by new { b.brand_name, c.category_name } into brandCategoryGroup
                                          select  new TotalsPerBrandCategory
                                          {
                                              Brand = brandCategoryGroup.Key.brand_name,
                                              Category = brandCategoryGroup.Key.category_name,
                                              TotalCount = brandCategoryGroup.Count() // Count of bicycles in each category per brand
                                          }).ToList();

            return View(totalsPerBrandCategory);
        }

        public ActionResult TotalsPerStore()
        {
            // Query to count bicycles available at each store location
            var totalsPerStore = (from oi in _dbContext.order_items
                                  join o in _dbContext.orders on oi.order_id equals o.order_id 
                                  join s in _dbContext.stores on o.store_id equals s.store_id 
                                  join p in _dbContext.products on oi.product_id equals p.product_id 
                                  group p by s.store_name into storeGroup 
                                  select new TotalsPerStore
                                  {
                                      Store = storeGroup.Key,
                                      TotalCount = storeGroup.Count() // Count of bicycles at each store
                                  }).ToList();

            return View(totalsPerStore);
        }


    }
}