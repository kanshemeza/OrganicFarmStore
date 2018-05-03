using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrganicFarmStore.Models;

namespace OrganicFarmStore.Controllers
{
    public class HomeController : Controller
    {
        private List<Product> _products;
        public HomeController()
        {
            _products = new List<Product>();
            {
                _products.Add(new Product
                {
                    ID = 4,
                    Name = "Green Bell Pepper",
                    Description = "Et nisl adhuc interpretaris cum, " +
                    "at mea equidem indoctum neglegentur. Odio eripuit suscipit sed no, etiam ",
                    Image = "/images/green-bell-pepper.jpg",
                    Price = 3.95m
                });

                _products.Add(new Product {
                    ID = 5,
                    Name = "Green Onions",
                    Description = "Sed ut perspiciatis, unde omnis iste natus error sit " +
                    "voluptatem accusantium doloremque laudantium",
                    Image = "/images/green-onions.jpg",
                    Price = 1.99m
                });

                _products.Add(new Product
                {
                    ID = 6,
                    Name = "Strawberries",
                    Description = "Et nisl adhuc interpretaris cum, " +
                    "at mea equidem indoctum neglegentur. Odio eripuit suscipit sed no, etiam ",
                    Image = "/images/strawberries.jpg",
                    Price = 3.95m
                });
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
                Product p = _products.Single(x => x.ID == id.Value);
                return View(p);
            }
            return NotFound();

        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
