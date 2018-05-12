using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OrganicFarmStore.Models;


namespace OrganicFarmStore.Controllers
{
    public class ProductController : Controller
    {
        private List<Product> _products;
        private string _AdventureWorks2016ConnectionString = null;
        private string _FinalProjDBConnectionString = null;

        public ProductController(IConfiguration config, IConfiguration fpconfig)
        {
            _AdventureWorks2016ConnectionString = config.GetConnectionString("AdventureWorks2016");
            _FinalProjDBConnectionString = fpconfig.GetConnectionString("FinalProjDB");
            _products = new List<Product>();

            //_products.Add(new Product()
            //{
            //    ID = 1,
            //    Name = "Ginger",
            //    Description = "Lorem ipsum dolor sit amet, " +
            //    "nec reque platonem philosophia ei. Dolores " +
            //    "salutandi voluptatibus sit no",
            //    Image = "/images/ginger-root.png",
            //    Price = 6.97m,
            //    Category = "Fresh"
            //});
            //_products.Add(new Product()
            //{
            //    ID = 2,
            //    Name = "Raspberry",
            //    Description = "Virtute propriae " +
            //    "honestatis ad vim, habeo inciderint adversarium vix ea, " +
            //    "luptatum reprehendunt an mea",
            //    Image = "/images/raspberries.jpg",
            //    Price = 2.97m,
            //    Category = "Fresh"
            //});
            //_products.Add(new Product
            //{
            //    ID = 3,
            //    Name = "HassAvocado",
            //    Description = "Virtute propriae " +
            //    "honestatis ad vim, habeo inciderint adversarium vix ea, " +
            //    "luptatum reprehendunt an mea",
            //    Image = "/images/HassAvocado.jpg",
            //    Price = 2.95m,
            //    Category = "Fresh"
            //});

            //string connectionString = @"Data Source = (localdb)\MSSQLLocalDB; Initial Catalog = AdventureWorks2016; Integrated Security = True; Connect Timeout = 30; Encrypt = False; TrustServerCertificate = True; ApplicationIntent = ReadWrite; MultiSubnetFailover = False";
            using (SqlConnection connection = new SqlConnection(_FinalProjDBConnectionString))
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = "sp_GetAllProducts";
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int productModelId = reader.GetInt32(0);
                            string name = reader.GetString(1);
                            string description = reader.GetString(2);
                            _products.Add(new Product
                            {
                                ID = productModelId,
                                Description = description,
                                Name = name
                            });
                        }
                    }
                }

                foreach (var product in _products)
                {
                    using (SqlCommand imageAndPriceCommand = connection.CreateCommand())
                    {
                        imageAndPriceCommand.CommandText = "sp_GetProductImages";
                        imageAndPriceCommand.CommandType = System.Data.CommandType.StoredProcedure;
                        imageAndPriceCommand.Parameters.AddWithValue("@productModelID", product.ID);
                        using (SqlDataReader reader2 = imageAndPriceCommand.ExecuteReader())
                        {
                            while (reader2.Read())
                            {
                                product.Price = reader2.IsDBNull(0) ? (decimal?)null : reader2.GetSqlMoney(0).ToDecimal();

                                byte[] imageBytes = (byte[])reader2[1];
                                product.Image = "data:image/jpeg;base64, " + Convert.ToBase64String(imageBytes);
                                break;
                            }
                        }
                    }
                }
            }
        }
        public IActionResult Index()
        {
            
            return View(_products);
            
        }

        public IActionResult Details(int? id)
        {
            if (id.HasValue)
            {
                //Product p = _products.Single(x => x.ID == id.Value);
                //return View(p);
                Product p = null;

                using (SqlConnection connection = new SqlConnection(_FinalProjDBConnectionString))
                { 
                 connection.Open();

                SqlCommand command = connection.CreateCommand();
                command.CommandText = "sp_GetProduct";
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@productModelID", id.Value);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            p = new Product
                            {
                                ID = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Description = reader.GetString(2)
                            };
                        }
                    }
                    if (p != null)
                    {
                        using (SqlCommand imageAndPriceCommand = connection.CreateCommand())
                        {

                            imageAndPriceCommand.CommandText = "sp_GetProductImages";
                            imageAndPriceCommand.CommandType = System.Data.CommandType.StoredProcedure;
                            imageAndPriceCommand.Parameters.AddWithValue("@productModelID", p.ID);
                            using (SqlDataReader reader2 = imageAndPriceCommand.ExecuteReader())
                            {
                                while (reader2.Read())
                                {
                                    p.Price = reader2.IsDBNull(0) ? (decimal?)null : reader2.GetSqlMoney(0).ToDecimal();
                                    byte[] imageBytes = (byte[])reader2[1];
                                    p.Image = "data:image/jpeg;base64, " + Convert.ToBase64String(imageBytes);
                                    break;
                                }
                            }
                        }
                    }
                }
                
                if(p != null)
                {
                    return View(p);
                }
            }
            return NotFound();
           
        }
        public IActionResult Inde()
        {
            
            return View();
        }
    }
}