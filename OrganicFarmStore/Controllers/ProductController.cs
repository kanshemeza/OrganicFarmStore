using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrganicFarmStore.Models;

namespace OrganicFarmStore.Controllers
{
    public class ProductController : Controller
    {
        private List<Product> _products;

        public ProductController()
        {
            _products = new List<Product>();

            _products.Add(new Product
            {
                ID = 1,
                Name = "Tomatoes",
                Description = "Lorem ipsum dolor sit amet, " +
                "nec reque platonem philosophia ei. Dolores " +
                "salutandi voluptatibus sit no",
                Image = "/images/tomatoes.jpg",
                Price = 6.97m,
                Category = "Fresh"
            });
            _products.Add(new Product
            {
                ID = 2,
                Name = "Raspberry",
                Description = "Virtute propriae " +
                "honestatis ad vim, habeo inciderint adversarium vix ea, " +
                "luptatum reprehendunt an mea",
                Image = "/images/raspberries.jpg",
                Price = 2.97m,
                Category = "Fresh"
            });
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
    }
}