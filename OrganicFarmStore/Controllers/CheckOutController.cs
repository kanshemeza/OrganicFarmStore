using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganicFarmStore.Models;

namespace OrganicFarmStore.Controllers
{
    public class CheckOutController : Controller
    {
        private readonly OrganicStoreDbContext _context;
        public CheckOutController(OrganicStoreDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public IActionResult Index(Checkout checkout)
        {
            if (ModelState.IsValid)
            {
                Guid cartId;
                Order order = new Order();
                if (Request.Cookies.ContainsKey("cartId"))
                {
                    if (Guid.TryParse(Request.Cookies["cartId"], out cartId))
                    {

                        Cart cart = _context.Carts
                            .Include(carts => carts.CartItems)
                            .ThenInclude(cartitems => cartitems.Product)
                            .FirstOrDefault(x => x.CookieIdentifier == cartId);

                        for (int i = 0; i < cart.CartItems.Count; i++)
                        {
                            order.OrderItems.Add(new OrderItem
                            {
                                Product = cart.CartItems.ElementAt(i).Product
                            });
                            
                        }
                        _context.Orders.Add(order);

                        _context.CartItems.RemoveRange(cart.CartItems);
                        _context.Carts.Remove(cart);

                        order.FirstName = checkout.FirstName;
                        order.LastName = checkout.LastName;
                        order.CreditCardNumber = checkout.CreditCardNumber;
                        order.ExpirationDate = checkout.ExpirationDate;
                        order.BillingAddress = checkout.BillingAddress;
                        order.City = checkout.City;
                        order.State = checkout.State;
                        order.PostalCode = checkout.PostalCode;
                        order.Phone = checkout.Phone;
                        order.Email = checkout.Email;


                        _context.SaveChanges();

                    }
                }
            }
            else
            {
                return View(checkout);
            }
            return RedirectToAction("Confirmation");

        }

        public IActionResult Confirmation()
        {

            return View();
        }
    }
}