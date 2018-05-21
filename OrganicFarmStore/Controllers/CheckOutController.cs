using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrganicFarmStore.Models;

namespace OrganicFarmStore.Controllers
{
    public class CheckOutController : Controller
    {
        string _sendGridKey;
        private readonly OrganicStoreDbContext _context;
        public CheckOutController(OrganicStoreDbContext context, IConfiguration configuration)
        {
            _context = context;
            _sendGridKey = configuration["SendGridKey"];
        }
        public IActionResult Index()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Checkout checkout, EmailReceipt model)
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

                #region use the SendGrid client to send a welcome email
                var client = new SendGrid.SendGridClient(_sendGridKey);
                var senderAddress = new SendGrid.Helpers.Mail.EmailAddress("admin@o-store.com", "Organic-Farm Store");
                var subject = "Your Receipt";
                var to = new SendGrid.Helpers.Mail.EmailAddress(model.Email, model.Email);
                var plainText = "Thanks for signing up, " + model.FirstName + "!";
                var htmlText = "<p> Thanks for Shopping with us, " + model.FirstName + "!</p>";
                var message = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(senderAddress, to, subject, plainText, htmlText);
                var mailResult = await client.SendEmailAsync(message);

                if ((mailResult.StatusCode == System.Net.HttpStatusCode.OK) || (mailResult.StatusCode == System.Net.HttpStatusCode.Accepted))
                    return RedirectToAction("Confirmation");
                else
                    return BadRequest(await mailResult.Body.ReadAsStringAsync());

                #endregion
            }
            else
            {
                return View(checkout);
            }

           

           // return RedirectToAction("Confirmation");

        }

        public IActionResult Confirmation()
        {

            return View();
        }
    }
}