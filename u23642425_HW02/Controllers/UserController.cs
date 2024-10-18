using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using u23642425_HW02.Models;
using PagedList;
using System.Data.Entity;


namespace u23642425_HW02.Controllers
{
    public class UserController : Controller
    {
        public BikeStoresDbContext _dbContext = new BikeStoresDbContext();

        static SqlConnection connection = new SqlConnection(Globals.connection);
        //List<Customer> customer = new List<Customer>();
        public ActionResult Register()
        {
            // Fetch customer types from the database
            var customerTypes = _dbContext.customertypes.ToList();

            // Create a SelectList and pass it to the ViewBag
            ViewBag.CustomerTypes = new SelectList(customerTypes, "customertype_id", "typename");

            return View();
          
        }
        [HttpPost]
        public ActionResult Register(customer customer)
        {
            try
            {
                // Add the customer entity to the DbSet and save changes
                _dbContext.customers.Add(customer);
                _dbContext.SaveChanges(); // Commit to the database
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                var customerTypes = _dbContext.customertypes.ToList();
                ViewBag.CustomerTypes = new SelectList(customerTypes, "customertype_id", "typename");

                return View(customer); // Return the view with the model so that validation errors can be displayed
            }

            TempData["RegisterSuccess"] = "You have successfully Registered!";
            return RedirectToAction("Login", "User");
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Login login)
        {
            try
            {
                // Modified query to select customer_id along with the customertype_id
                string query = "SELECT customer_id, customertype_id FROM sales.customers WHERE email = @Email AND zip_code = @ZipCode";
                SqlCommand searchlogin = new SqlCommand(query, connection);

                searchlogin.Parameters.AddWithValue("@Email", login.Email);
                searchlogin.Parameters.AddWithValue("@ZipCode", login.ZipCode);

                connection.Open();
                SqlDataReader reader = searchlogin.ExecuteReader();

                if (reader.Read())
                {
                    // Successfully logged in, retrieve values
                    int customerId = reader.GetInt32(0); // Get customer_id
                    int customerType = reader.GetInt32(1); // Get customertype_id

                    // Store values in session
                    Session["IsLoggedIn"] = true;
                    Session["CustomerId"] = customerId;
                    Session["CustomerType"] = customerType;

                    // Store success message in TempData
                    TempData["LoginSuccess"] = "You have successfully logged in!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt. Please register if you don't have an account. " +
                                                 "<a href='" + Url.Action("Register", "User") + "'>Register here</a>.");
                    return View();  // Return to the Login view
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();  // Return to the Login view
            }
            finally
            {
                connection.Close();
            }
        }


        public ActionResult Logout()
        {
            Session["IsLoggedIn"] = null;
            Session.Clear();
            return RedirectToAction("Login");
        }




        public ActionResult Buy(decimal? price, string brand, string brandCategory, string storeName)
        {
            // Join the tables and fetch necessary fields
            var productsQuery = from s in _dbContext.stocks
                                join p in _dbContext.products on s.product_id equals p.product_id
                                join b in _dbContext.brands on p.brand_id equals b.brand_id
                                join c in _dbContext.categories on p.category_id equals c.category_id
                                join st in _dbContext.stores on s.store_id equals st.store_id
                                select new
                                {
                                    p,
                                    b,
                                    c,
                                    s,
                                    st
                                };

            // Apply filtering by price if provided
            if (price.HasValue)
            {
                productsQuery = productsQuery.Where(x => x.p.list_price >= price.Value);
            }

            // Apply filtering by brand if provided
            if (!string.IsNullOrEmpty(brand))
            {
                productsQuery = productsQuery.Where(x => x.b.brand_name == brand);
            }

            // Apply filtering by brand category if provided
            if (!string.IsNullOrEmpty(brandCategory))
            {
                productsQuery = productsQuery.Where(x => x.c.category_name == brandCategory);
            }

            // Apply filtering by store location if provided
            if (!string.IsNullOrEmpty(storeName))
            {
                productsQuery = productsQuery.Where(x => x.st.store_name == storeName);
            }

            // Execute the query and get the filtered results
            var filteredProducts = productsQuery.Select(x => x.p).ToList();

            return View(filteredProducts);
        }

        [HttpPost]
        public ActionResult PurchaseOrder(int productid, int quantity)
        {
            // Fetch the product from the database
            var product = _dbContext.products.FirstOrDefault(p => p.product_id == productid);
            if (product == null)
            {
                return HttpNotFound("Bike not available, try again later");
            }

            var customerid = (int)Session["CustomerId"]; 
            if (customerid == 0)
            {
                return HttpNotFound("Customer not logged in.");
            }

            var stock = _dbContext.stocks.FirstOrDefault(s => s.product_id == productid);
            if (stock == null)
            {
                return HttpNotFound("No stock available for this bike.");
            }

            var store = _dbContext.stores.FirstOrDefault(s => s.store_id == stock.store_id);
            if (store == null)
            {
                return HttpNotFound("Store not found.");
            }

            // Assuming your store or another table has a staff_id associated with it.
            var staffid = _dbContext.staffs.FirstOrDefault(s => s.store_id == store.store_id)?.staff_id;
            if (staffid == null)
            {
                return HttpNotFound("Staff not available.");
            }

            // Use Entity Framework to create a new order
            var order = new order
            {
                customer_id = customerid,
                order_status = 4, // constant
                order_date = DateTime.Now,
                required_date = DateTime.Now.AddDays(7), // Just added 7 days
                store_id = store.store_id,
                staff_id = staffid.Value 
            };

            // Add the order to the context and save changes to generate order_id
            _dbContext.orders.Add(order);
            _dbContext.SaveChanges(); 

            // Ensure the order ID is properly generated
            if (order.order_id == 0)
            {
                throw new Exception("Order ID was not generated.");
            }

            // Generate a random discount for this order item
            Random random = new Random();
            decimal randomDiscountPercentage = random.Next(0, 21) / 100m; // Discount between 0 and 20%
            decimal randomDiscountAmount = product.list_price * randomDiscountPercentage;

            // Use Entity Framework model for order items
            var orderItem = new order_items
            {
                order_id = order.order_id,  // This is set from the saved order
                product_id = product.product_id,
                quantity = quantity,
                list_price = Math.Round(product.list_price, 2), // Round to 2 decimal places
                discount = Math.Round(randomDiscountAmount, 2)
            };

            // Add the order item and save changes
            _dbContext.order_items.Add(orderItem);
            _dbContext.SaveChanges(); 

            // Optionally, calculate the total order amount based on the order items
            var totalAmount = _dbContext.order_items
                .Where(oi => oi.order_id == order.order_id)
                .Sum(oi => (oi.list_price * oi.quantity) - oi.discount);

            // Redirect to the home page after successful purchase
            return RedirectToAction("Confirmation", new { orderId = order.order_id });
        }

        public ActionResult Confirmation(int orderId)
        {
            // Perform a join between orders, order_items, products, customers, and stores
            var orderDetails = (from o in _dbContext.orders
                                join oi in _dbContext.order_items on o.order_id equals oi.order_id
                                join p in _dbContext.products on oi.product_id equals p.product_id
                                join c in _dbContext.customers on o.customer_id equals c.customer_id
                                join s in _dbContext.stores on o.store_id equals s.store_id
                                where o.order_id == orderId
                                select new
                                {
                                    o.order_id,
                                    o.customer_id,
                                    o.order_date,
                                    o.required_date,
                                    oi.quantity,
                                    oi.list_price,
                                    oi.discount,
                                    ProductName = p.product_name,
                                    CustomerName = c.first_name,
                                    Surname = c.last_name,   
                                    StoreName = s.store_name,        
                                    StorePhone = s.phone,
                                    StoreEmail = s.email,
                                    StoreStreet = s.street,
                                    StoreCity = s.city,
                                    StoreState = s.state,
                                    StoreZip = s.zip_code
                                }).ToList();

            // If no order details found, return a 404 error
            if (!orderDetails.Any())
            {
                return HttpNotFound("Order not found.");
            }

            // Extract the order's basic info
            var firstOrder = orderDetails.First();

            // Create the view model
            var viewModel = new OrderConfirmationView
            {
                OrderId = firstOrder.order_id,
                CustomerId = (int)firstOrder.customer_id,
                CustomerName = firstOrder.CustomerName,
                Surnamee = firstOrder.Surname,// Adding customer name to ViewModel
                OrderDate = firstOrder.order_date,
                RequiredDate = firstOrder.required_date,
                TotalAmount = (decimal)orderDetails.Sum(oi => (oi.list_price * oi.quantity) - oi.discount),
                StoreDetails = new StoreViewModel            // Adding store details to ViewModel
                {
                    StoreName = firstOrder.StoreName,
                    Phone = firstOrder.StorePhone,
                    Email = firstOrder.StoreEmail,
                    Street = firstOrder.StoreStreet,
                    City = firstOrder.StoreCity,
                    State = firstOrder.StoreState,
                    ZipCode = firstOrder.StoreZip
                },
                OrderItems = orderDetails.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.ProductName,
                    Quantity = oi.quantity,
                    ListPrice = (decimal)oi.list_price,
                    Discount = (decimal)oi.discount
                }).ToList()
            };

            return View(viewModel);
        }



        public ActionResult Index(int? page)
        {
            // Assuming you store customer ID in the session when the user logs in
            int? customerId = Session["CustomerId"] as int?;

            // If no customer is logged in, redirect to the login page
            if (customerId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Fetch products related to the logged-in customer, ordered by product_id in descending order
            var customerProducts = _dbContext.products
                                              .Where(p => p.customer_id == customerId)
                                              .OrderByDescending(p => p.product_id)
                                              .ToList();

            // Set page size and calculate the page number
            int pageSize = 10; // Number of items per page
            int pageNumber = (page ?? 1); // Default to page 1 if null

            // Get a list of similar bikes based on the first customer's product
            List<product> similarBikes = new List<product>();

            if (customerProducts.Any())
            {
                // Get the first product from the customer's products for comparison
                var currentProduct = customerProducts.First();

                // Find similar bikes
                similarBikes = _dbContext.products
                    .Where(p => (p.brand_id == currentProduct.brand_id ||
                                  Math.Abs(p.list_price - currentProduct.list_price) < 500) &&
                                 p.product_id != currentProduct.product_id) // Exclude the current product
                    .ToList();
            }

            // Return paged list and similar bikes
            ViewBag.SimilarBikes = similarBikes; // Pass similar bikes to the view
            return View(customerProducts.ToPagedList(pageNumber, pageSize));
        }


        public ActionResult Sell()
        {
            // Populate BrandList and CategoryList for the dropdowns
            ViewBag.BrandList = new SelectList(_dbContext.brands, "brand_id", "brand_name");
            ViewBag.CategoryList = new SelectList(_dbContext.categories, "category_id", "category_name");

            return View();
        }


        [HttpPost]
        public ActionResult Sell(string productName, int brandId, int categoryId, int modelYear, decimal listPrice, HttpPostedFileBase ImageFile, int customer_id)
        {
            if (ModelState.IsValid)
            {
                // Create a new product object
                var newProduct = new product
                {
                    product_name = productName,
                    brand_id = brandId,
                    category_id = categoryId,
                    model_year = (short)modelYear,
                    list_price = listPrice,
                    customer_id = customer_id
                };

                // If an image file is uploaded
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        ImageFile.InputStream.CopyTo(ms);
                        newProduct.ImageData = ms.ToArray();  // Store the image as binary data
                    }
                }

                // Save the product to the database
                _dbContext.products.Add(newProduct);
                _dbContext.SaveChanges();

                TempData["SuccessMessage"] = "Product successfully placed for sale!";
               

                return RedirectToAction("Index");
            }

            // Repopulate the dropdowns if model state is invalid
            ViewBag.BrandList = new SelectList(_dbContext.brands, "brand_id", "brand_name");
            ViewBag.CategoryList = new SelectList(_dbContext.categories, "category_id", "category_name");

            return View();
        }



        public ActionResult Edit(int id)
        {
            var product = _dbContext.products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            // Populate BrandList and CategoryList for the dropdowns
            ViewBag.BrandList = new SelectList(_dbContext.brands, "brand_id", "brand_name", product.brand_id);
            ViewBag.CategoryList = new SelectList(_dbContext.categories, "category_id", "category_name", product.category_id);

            return View(product);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( int product_id,string product_name,int brand_id,int category_id, int model_year,decimal list_price,HttpPostedFileBase ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Find the existing product from the database
                var productInDb = _dbContext.products.Find(product_id);
                if (productInDb == null)
                {
                    return HttpNotFound();
                }

                // Update the fields
                productInDb.product_name = product_name;
                productInDb.brand_id = brand_id;
                productInDb.category_id = category_id;
                productInDb.model_year = (short)model_year;
                productInDb.list_price = list_price;

                // Handle image upload
                if (ImageFile != null && ImageFile.ContentLength > 0)
                {
                    using (var binaryReader = new BinaryReader(ImageFile.InputStream))
                    {
                        productInDb.ImageData = binaryReader.ReadBytes(ImageFile.ContentLength);
                    }
                }

                // Save changes to the database
                _dbContext.SaveChanges();

                TempData["SuccessMessage"] = "Product successfully Updated!";
                // Redirect to Index after successful update
                return RedirectToAction("Index", "User");
            }

            // If validation fails, reload the view with the same data
            ViewBag.BrandList = new SelectList(_dbContext.brands, "brand_id", "brand_name", brand_id);
            ViewBag.CategoryList = new SelectList(_dbContext.categories, "category_id", "category_name", category_id);

            return View();
        }



        //// GET: User/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    var product = _dbContext.products.Find(id);
        //    if (product == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(product);
        //}

        // POST: User/Delete
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var product = _dbContext.products.Find(id);
            if (product != null)
            {
                _dbContext.products.Remove(product);
                _dbContext.SaveChanges();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Product not found." });
        }

        public ActionResult MyBikes()
        {
            int customerId = Convert.ToInt32(Session["CustomerId"]);

            // Query to find all products bought or sold by the user.
            var productsQuery = from p in _dbContext.products
                                join oi in _dbContext.order_items on p.product_id equals oi.product_id
                                join o in _dbContext.orders on oi.order_id equals o.order_id
                                where p.customer_id == customerId || o.customer_id == customerId
                                select new
                                {
                                    Product = p,
                                    IsBuyer = o.customer_id == customerId,
                                    IsSeller = p.customer_id == customerId
                                };

            var myBikes = productsQuery.ToList();

            // Pass anonymous objects to the view, map them to a model directly in the view
            return View(myBikes.Select(p => p.Product).ToList());
        }



    }
}