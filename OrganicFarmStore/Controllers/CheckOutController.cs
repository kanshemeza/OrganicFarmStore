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
        private SmartyStreets.USStreetApi.Client _usStreetApiClient;

        public CheckOutController(OrganicStoreDbContext context,
            IConfiguration configuration,
            BraintreeGateway brainTreeGateway,
            SignInManager<OrganicStoreUser> signInManager,
            EmailService emailService,
        SmartyStreets.USStreetApi.Client usStreetApiClient)
        {
            _context = context;
            _sendGridKey = configuration["SendGridKey"];
            _brainTreeGateway = brainTreeGateway;
            _signInManager = signInManager;
            _emailService = emailService;
            _usStreetApiClient = usStreetApiClient;
        }
        public async Task<IActionResult> Index()
        {
            Checkout model = new Checkout();
            await GetCurrentCart(model);

            if (User.Identity.IsAuthenticated)
            {
                OrganicStoreUser currentUser = await _signInManager.UserManager.GetUserAsync(User);
                Braintree.CustomerSearchRequest search = new Braintree.CustomerSearchRequest();
                search.Email.Is(currentUser.Email);
                var searchResult = await _brainTreeGateway.Customer.SearchAsync(search);
                if (searchResult.Ids.Count > 0)
                {
                    Braintree.Customer customer = searchResult.FirstItem;
                    model.CreditCards = customer.CreditCards;
                    model.Addresses = customer.Addresses;
                }
            }
            if (model.Cart == null)
            {
                return RedirectToAction("Index", "Home");
            }

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
        public async Task<IActionResult> Index(Checkout model)
        {
            await GetCurrentCart(model);
            model.Addresses = new Address[0];

            if (ModelState.IsValid)
            {
               if (!string.IsNullOrEmpty(model.SavedAddressId) ||
                    (!string.IsNullOrEmpty(model.ShippingAddressLine1) && !string.IsNullOrEmpty(model.ShippingLocale)
                    && !string.IsNullOrEmpty(model.ShippingRegion) && !string.IsNullOrEmpty(model.ShippingPostalCode) && !string.IsNullOrEmpty(model.ShippingCountry)))
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

                    Braintree.Customer customer = null;
                    Braintree.CustomerSearchRequest search = new Braintree.CustomerSearchRequest();
                    search.Email.Is(model.ContactEmail);
                    var searchResult = await _brainTreeGateway.Customer.SearchAsync(search);
                    if (searchResult.Ids.Count == 0)
                    {
                        //Create  a new Braintree Customer
                        Braintree.Result<Customer> creationResult = await _brainTreeGateway.Customer.CreateAsync(new Braintree.CustomerRequest
                        {
                            Email = model.ContactEmail,
                            Phone = model.ContactPhoneNumber
                        });
                        customer = creationResult.Target;
                    }
                    else
                    {
                        customer = searchResult.FirstItem;
                    }

                    CreditCard creditCard = null;
                    if (model.SaveBillingCard)
                    {
                        var newCardRequest = new CreditCardRequest
                        {
                            CardholderName = model.BillingNameOnCard,
                            CustomerId = customer.Id,
                            ExpirationMonth = model.BillingCardExpirationMonth.ToString().PadLeft(2, '0'),
                            ExpirationYear = model.BillingCardExpirationYear.ToString(),
                            Number = model.BillingCardNumber,
                            CVV = model.BillingCardVerificationValue
                        };
                        var newCardResult = await _brainTreeGateway.CreditCard.CreateAsync(newCardRequest);
                        if (newCardResult.IsSuccess())
                        {
                            creditCard = newCardResult.Target;
                        }
                    }

                    Address savedAddress = null;
                    if (model.SaveShippingAddress)
                    {
                        var newAddressRequest = new AddressRequest
                        {
                            StreetAddress = model.ShippingAddressLine1,
                            ExtendedAddress = model.ShippingAddressLine2,
                            CountryName = model.ShippingCountry,
                            PostalCode = model.ShippingPostalCode,
                            Locality = model.ShippingLocale,
                            Region = model.ShippingRegion
                        };
                        var newAddressResult = await _brainTreeGateway.Address.CreateAsync(customer.Id, newAddressRequest);
                        if (newAddressResult.IsSuccess())
                        {
                            savedAddress = newAddressResult.Target;
                        }
                    }

                    TransactionRequest transaction = new TransactionRequest
                    {
                        Amount = model.Cart.CartItems.Sum(x => x.Quantity * (x.Product.Price ?? 0)),


                        CustomerId = customer.Id,
                        LineItems = model.Cart.CartItems.Select(x => new TransactionLineItemRequest
                        {
                            Name = x.Product.Name,
                            Description = x.Product.Description,
                            ProductCode = x.Product.ID.ToString(),
                            Quantity = x.Quantity,
                            LineItemKind = TransactionLineItemKind.DEBIT,
                            UnitAmount = x.Product.Price * x.Quantity,
                            TotalAmount = x.Product.Price * x.Quantity
                        }).ToArray()
                    };

                    if (creditCard == null)
                    {
                        transaction.CreditCard = new TransactionCreditCardRequest
                        {
                            Number = model.BillingCardNumber,
                            CardholderName = model.BillingNameOnCard,
                            CVV = model.BillingCardVerificationValue,
                            ExpirationMonth = model.BillingCardExpirationMonth.ToString().PadLeft(2, '0'),
                            ExpirationYear = model.BillingCardExpirationYear.ToString()
                        };
                    }
                    else
                    {
                        transaction.PaymentMethodToken = creditCard.Token;
                    }
                    if (savedAddress != null)
                    {
                        transaction.ShippingAddressId = savedAddress.Id;
                    }


                    var transactionResult = await _brainTreeGateway.Transaction.SaleAsync(transaction);
                    if (transactionResult.IsSuccess())
                    {

                        _context.Orders.Add(neworder);

                        _context.CartItems.RemoveRange(model.Cart.CartItems);
                        _context.Carts.Remove(model.Cart);


                        await _context.SaveChangesAsync();

                        Response.Cookies.Delete("cartId");

                        RegisterViewModel regModel = new RegisterViewModel();
                        var plainText = "Thanks for signing up, " + regModel.FirstName + "!";
                        var htmlText = "<p> Thanks for Shopping with us, " + regModel.FirstName + "!</p>";
                        
                        await _emailService.SendEmailAsync(model.ContactEmail, "Your Receipt", htmlText, plainText);



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
            }
            return View(model);
      }
        
        

        public IActionResult Confirmation()
        {

            return View();
        }

        public IActionResult ValidateAddress(string addressLine1, string addressLine2, string region, string locale, string country, string postalCode)
        {
            if (country == "United States of America")
            {
                var lookup = new SmartyStreets.USStreetApi.Lookup
                {
                    City = locale,
                    State = region,
                    Street = addressLine1,
                    Street2 = addressLine2,
                    ZipCode = postalCode
                };
                _usStreetApiClient.Send(lookup);

                return Json(lookup.Result.Select(x => new
                {
                    AddressLine1 = x.DeliveryLine1,
                    AddressLine2 = x.DeliveryLine2,
                    PostalCode = x.Components.ZipCode,
                    Region = x.Components.State,
                    Locale = x.Components.CityName
                }));
            }
            else
            {
                return Json(new[]{ new {
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            PostalCode = postalCode,
            Region = region,
            Locale = locale
        }});
            }
        }
    }
}