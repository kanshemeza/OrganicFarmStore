using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Braintree;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrganicFarmStore.Models;

namespace OrganicFarmStore.Controllers
{
    public class CheckOutController : Controller
    {
        string _sendGridKey;
        private  BraintreeGateway _brainTreeGateway;
        private EmailService _emailService;
        private SignInManager<OrganicStoreUser> _signInManager;
        private readonly OrganicStoreDbContext _context;

        public CheckOutController(OrganicStoreDbContext context, 
            IConfiguration configuration, 
            BraintreeGateway brainTreeGateway, 
            SignInManager<OrganicStoreUser> signInManager, 
            EmailService emailService)
        {
            _context = context;
            _sendGridKey = configuration["SendGridKey"];
            _brainTreeGateway = brainTreeGateway;
            _signInManager = signInManager;
            _emailService = emailService;
        }
        public async Task<IActionResult> Index()
        {
            Checkout model = new Checkout();
            await GetCurrentCart(model);
            return View(model);
        }

        private async Task GetCurrentCart(Checkout model)
        {
            Guid cartId;
            Cart cart = null;

            if (User.Identity.IsAuthenticated)
            {
                var currentUser = await _signInManager.UserManager.GetUserAsync(User);
                model.ContactEmail = currentUser.Email;
                model.ContactPhoneNumber = currentUser.PhoneNumber;
            }

            if (Request.Cookies.ContainsKey("cartId"))
            {
                if (Guid.TryParse(Request.Cookies["cartId"], out cartId))
                {
                    cart = await _context.Carts
                        .Include(carts => carts.CartItems)
                        .ThenInclude(cartitems => cartitems.Product)
                        .FirstOrDefaultAsync(x => x.CookieIdentifier == cartId);
                }
            }
            model.Cart = cart;
        }
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Index(Checkout model, EmailReceipt emailReceipt)
        {
            await GetCurrentCart(model);
            if (ModelState.IsValid)
            {
                Order neworder = new Order
                {
                    TrackingNumber = Guid.NewGuid().ToString(),
                    OrderDate = DateTime.Now,
                    OrderItems = model.Cart.CartItems.Select(x => new OrderItem
                    {
                        ProductID = x.Product.ID,
                        ProductName = x.Product.Name,
                        ProductPrice = (x.Product.Price ?? 0),
                        Quantity = x.Quantity
                    }).ToArray(),
                    AddressLine1 = model.ShippingAddressLine1,
                    AddressLine2 = model.ShippingAddressLine2,
                    Country = model.ShippingCountry,
                    Email = model.ContactEmail,
                    PhoneNumber = model.ContactPhoneNumber,
                    Locale = model.ShippingLocale,
                    PostalCode = model.ShippingPostalCode,
                    Region = model.ShippingRegion
                };

                TransactionRequest transaction = new TransactionRequest
                {
                    Amount = model.Cart.CartItems.Sum(x => x.Quantity * (x.Product.Price ?? 0)),
                    CreditCard = new TransactionCreditCardRequest
                    {
                        Number = model.BillingCardNumber,
                        CardholderName = model.BillingNameOnCard,
                        CVV = model.BillingCardVerificationValue,
                        ExpirationMonth = model.BillingCardExpirationMonth.ToString().PadLeft(2, '0'),
                        ExpirationYear = model.BillingCardExpirationYear.ToString()

                    }
                };

                var transactionResult = await _brainTreeGateway.Transaction.SaleAsync(transaction);
                if (transactionResult.IsSuccess())
                {

                    _context.Orders.Add(neworder);

                    _context.CartItems.RemoveRange(model.Cart.CartItems);
                    _context.Carts.Remove(model.Cart);


                    await _context.SaveChangesAsync();

                    Response.Cookies.Delete("cartId");

                    var plainText = "Thanks for signing up, " + emailReceipt.FirstName + "!";
                    var htmlText = "<p> Thanks for Shopping with us, " + emailReceipt.FirstName + "!</p>";

                    await _emailService.SendEmailAsync(emailReceipt.Email, "Your Receipt", htmlText, plainText);



                    return RedirectToAction("Confirmation", "Checkout", new { id = neworder.TrackingNumber });


                }
                for (int i = 0; i < transactionResult.Errors.Count; i++)
                {
                    ModelState.AddModelError("BillingCardNumber" + i, transactionResult.Errors.All()[i].Message);
                }

            }
            //#region use the SendGrid client to send a welcome email
            //var client = new SendGrid.SendGridClient(_sendGridKey);
            //var senderAddress = new SendGrid.Helpers.Mail.EmailAddress("admin@o-store.com", "Organic-Farm Store");
            //var subject = "Your Receipt";
            //var to = new SendGrid.Helpers.Mail.EmailAddress(emailReceipt.Email, emailReceipt.Email);
            //var plainText = "Thanks for signing up, " + emailReceipt.FirstName + "!";
            //var htmlText = "<p> Thanks for Shopping with us, " + emailReceipt.FirstName + "!</p>";
            //var message = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(senderAddress, to, subject, plainText, htmlText);
            //var mailResult = await client.SendEmailAsync(message);

            //if ((mailResult.StatusCode == System.Net.HttpStatusCode.OK) || (mailResult.StatusCode == System.Net.HttpStatusCode.Accepted))
            //{
            //    return RedirectToAction("Confirmation");
            //}

            //else
            //{
            //    return BadRequest(await mailResult.Body.ReadAsStringAsync());
            //}


            //#endregion
        
            return View(model);
      }
        
        

        public IActionResult Confirmation()
        {

            return View();
        }
    }
}