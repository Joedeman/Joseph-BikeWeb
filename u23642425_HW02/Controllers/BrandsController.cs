using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using u23642425_HW02.Models;
using System.Web.Configuration;

namespace u23642425_HW02.Controllers
{
    public class BrandsController : Controller
    {
        static SqlConnection connection = new SqlConnection(Globals.connection);

        //public ActionResult InsertImages()
        //{
        //    string folderPath = Server.MapPath("~/Content/BinaryImages");
        //    ViewBag.Messages = ""; // Initialize the ViewBag for messages

        //    try
        //    {
        //        connection.Open(); // Open the connection before the loop

        //        for (int i = 1; i <= 9; i++)
        //        {
        //            // Update to search for .jpeg files instead of .jpg
        //            string imagePath = Path.Combine(folderPath, i.ToString() + ".jpeg");

        //            // Log the image path to the ViewBag to confirm it is correct
        //            ViewBag.Messages += $"Checking for image: {imagePath}<br/>";

        //            if (System.IO.File.Exists(imagePath))
        //            {
        //                // Convert the image to byte array using a memory stream
        //                byte[] imageData = ConvertImageToByteArray(Image.FromFile(imagePath), ImageFormat.Jpeg);

        //                string query = "UPDATE [production].[products] SET ImageData = @ImageData WHERE brand_id = @BrandID";
        //                SqlCommand img = new SqlCommand(query, connection);

        //                img.Parameters.Add("@ImageData", SqlDbType.Image).Value = imageData;
        //                img.Parameters.AddWithValue("@BrandID", i);

        //                int rowsAffected = img.ExecuteNonQuery(); // Execute the command

        //                ViewBag.Messages += $"Image {i}.jpeg successfully inserted. Rows affected: {rowsAffected}<br/>";
        //            }
        //            else
        //            {
        //                ViewBag.Messages += $"Image {i}.jpeg not found.<br/>";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Messages += $"An error occurred: {ex.Message}<br/>";
        //    }
        //    finally
        //    {
        //        connection.Close(); // Close the connection after the loop
        //    }

        //    return View();
        //}

        // Helper method to convert the image to byte array
        private byte[] ConvertImageToByteArray(Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format); // Save the image to the memory stream
                return ms.ToArray(); // Return the byte array
            }
        }


        public ActionResult BrandProducts(int brandId)
        {
            List<Products> products = new List<Products>();

            try
            {
                connection.Open();

                string query = "SELECT product_id, product_name, brand_id, category_id, model_year, list_price, ImageData " +
                               "FROM [production].[products] WHERE brand_id = @BrandID";

                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@BrandID", brandId);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    Products product = new Products
                    {
                        ProductID = reader.GetInt32(0), // product_id is int
                        ProductName = reader.GetString(1), // product_name is string
                        BrandID = reader.GetInt32(2), // brand_id is int
                        CategoryID = reader.GetInt32(3), // category_id is int
                        ModelYear = reader.GetInt16(4), // model_year is smallint (short in C#)
                        ListPrice = reader.GetDecimal(5), // list_price is decimal
                        ImageData = reader["ImageData"] as byte[] // ImageData is varbinary or image
                    };

                    products.Add(product);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                ViewBag.Message = "An error occurred: " + ex.Message;
            }
            finally
            {
                connection.Close();
            }

            return View(products); // Pass the list of products to the view
        }


       public BikeStoresDbContext _dbContext = new BikeStoresDbContext();
        public ActionResult Stores()
        {
            var allProducts = _dbContext.products.Include(p => p.category).ToList();
            return View(allProducts);
        }

        public ActionResult PriceSearch(decimal? minPrice,decimal? maxPrice)
        {
            minPrice = minPrice ?? 100;   // Default to 100 if no minPrice is provided
            maxPrice = maxPrice ?? 10000; // Default to 10000 if no maxPrice is provided

            var filteredBikes = _dbContext.products.Include(c => c.category).ToList()
                                .Where(p => p.list_price >= minPrice && p.list_price <= maxPrice)
                                .OrderBy(p => p.list_price)
                                .ToList();

            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(filteredBikes);
        }


        public ActionResult ShopGlobally(int storeid)
        {
            var products = (from s in _dbContext.stocks
                            join p in _dbContext.products
                            on s.product_id equals p.product_id
                            join b in _dbContext.brands
                            on p.brand_id equals b.brand_id
                            join c in _dbContext.categories
                            on p.category_id equals c.category_id
                            where s.store_id == storeid
                            select new ProductViewModel
                            {
                                ProductName = p.product_name,
                                BrandName = b.brand_name,
                                CategoryName = c.category_name,
                                ModelYear = p.model_year,
                                ListPrice = p.list_price,
                                ImageData = p.ImageData,
                                Quantity = s.quantity
                            }).ToList();

            return View(products);
        }


         public ActionResult Bikes()
        {
            var Bikes = _dbContext.products.Include(c => c.category).ToList()
                                .ToList();
            return View(Bikes);
        }

    }

}

