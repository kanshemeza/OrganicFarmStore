using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OrganicFarmStore.Models;


namespace OrganicFarmStore.Controllers
{
    public class AccountController : Controller
    {
        SignInManager<OrganicStoreUser> _signInManager;
        //string _sendGridKey;
        EmailService _emailService;
        //public AccountController(SignInManager<OrganicStoreUser> signInManager, IConfiguration configuration)
        //{
        //    this._signInManager = signInManager;
        //    this._sendGridKey = configuration["SendGridKey"];
        //}
        public AccountController(SignInManager<OrganicStoreUser> signInManager, EmailService emailService)
        {
            this._signInManager = signInManager;
            this._emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        //Responds on GET of /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        //Responds on POST of /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken] //Demands the right token from the submitted page by making sure original user heuristics are the same
        public async Task<IActionResult> Register(RegisterViewModel model) //We're binding the RegisterViewModel model class to access it's properties
        {
            if (ModelState.IsValid)
            {
                //TODO: Create an account and log him in
                //  OrganicStoreUser newUser = new OrganicStoreUser(model.UserName);
                OrganicStoreUser newUser = new OrganicStoreUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                IdentityResult creationResult =await _signInManager.UserManager.CreateAsync(newUser);

                if (creationResult.Succeeded)
                {
                    //TODO: Create an account and log this user in
                    IdentityResult passwordResult = await this._signInManager.UserManager.AddPasswordAsync(newUser, model.Password);
                    if (passwordResult.Succeeded)
                    {
                        var confirmationToken = await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(newUser);
                        
                        confirmationToken = System.Net.WebUtility.UrlEncode(confirmationToken); // This will format our token which might have the plus signs, dashes, etc
                        
                        string currentUrl = Request.GetDisplayUrl();    //This will get me the URL for the current request
                        System.Uri uri = new Uri(currentUrl);   //This will wrap it in a "URI" object so I can split it into parts
                        string confirmationUrl = uri.GetLeftPart(UriPartial.Authority); //This gives me just the scheme + authority of the URI
                        confirmationUrl += "/account/confirm?id=" + confirmationToken + "&userId=" + System.Net.WebUtility.UrlEncode(newUser.Id);

                        #region use the SendGrid client to send a welcome email
                        var mailResult = await _emailService.SendEmailAsync(
                        model.Email,
                        "Welcome to BikeStore!",
                         "<p>Thanks for signing up, " + model.UserName + "!</p><p><a href=\"" + confirmationUrl + "\">Confirm your account<a></p>",
                          "Thanks for signing up, " + model.UserName + "!"
                        //"Thanks for signing up, " + model.UserName + "!",
                        //"<p>Thanks for signing up, " + model.UserName + "!</p>"
                    );
                        if (mailResult.Success)
                            return RedirectToAction("RegisterConfirmation");
                        else
                            return BadRequest(mailResult.Message);
                        #endregion

                        //#region use the SendGrid client to send a welcome email
                        //var client = new SendGrid.SendGridClient(_sendGridKey);
                        //var senderAddress = new SendGrid.Helpers.Mail.EmailAddress("admin@ctostore.com", "CT O-Store");
                        //var subject = "Welcome to OrganicStore";
                        //var to = new SendGrid.Helpers.Mail.EmailAddress(model.Email, model.Email);
                        //var plainText = "Thanks for signing up, " + model.FirstName + "!";
                        //var htmlText = "<p> Thanks for signing up with us, " + model.FirstName + "!</p>";
                        //var message = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(senderAddress, to, subject, plainText, htmlText);
                        //var mailResult = await client.SendEmailAsync(message);

                        //if ((mailResult.StatusCode == System.Net.HttpStatusCode.OK) || (mailResult.StatusCode == System.Net.HttpStatusCode.Accepted))
                        //    return RedirectToAction("RegisterConfirmation");
                        //else
                        //    return BadRequest(await mailResult.Body.ReadAsStringAsync());

                        //#endregion

                        //this._signInManager.SignInAsync(newUser, false);
                        // return RedirectToAction("SignIn", "Account");
                    }
                    else
                    {
                        foreach(var error in passwordResult.Errors)
                        {
                            ModelState.AddModelError(error.Code, error.Description);
                        }
                    }

                    
                }
                else
                {
                    foreach(var error in creationResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                }

               // return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public IActionResult RegisterConfirmation()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            if (ModelState.IsValid)
            {
                // IdentityUser existingUser = _signInManager.UserManager.FindByNameAsync(email).Result;
                OrganicStoreUser existingUser = await _signInManager.UserManager.FindByNameAsync(email);
                if (existingUser != null)
                {
                    Microsoft.AspNetCore.Identity.SignInResult passwordResult =await this._signInManager.CheckPasswordSignInAsync(existingUser, password, false);
                    if (passwordResult.Succeeded)
                    {
                       await _signInManager.SignInAsync(existingUser, false);
                        return RedirectToAction("Index", "Product");
                    }
                    else
                    {
                        ModelState.AddModelError("password"," Wrong username or password");
                    }
                }
                else
                {
                    ModelState.AddModelError("username", "Wrong username or password");
                }
            }
            return View();
        }
        public async Task<IActionResult> SignOut()
        {
            await this._signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if ((ModelState.IsValid) && (!string.IsNullOrEmpty(email)))
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(email);
                if (user != null)
                {
                    var resetToken = await _signInManager.UserManager.GeneratePasswordResetTokenAsync(user);

                    resetToken = System.Net.WebUtility.UrlEncode(resetToken);
                    //using Microsoft.AspNetCore.Http.Extensions;
                    string currentUrl = Request.GetDisplayUrl();    //This will get me the URL for the current request
                    System.Uri uri = new Uri(currentUrl);   //This will wrap it in a "URI" object so I can split it into parts
                    string resetUrl = uri.GetLeftPart(UriPartial.Authority); //This gives me just the scheme + authority of the URI
                    resetUrl += "/account/resetpassword?id=" + resetToken + "&userId=" + System.Net.WebUtility.UrlEncode(user.Id);

                    string htmlContent = "<a href=\"" + resetUrl + "\">Reset your password</a>";
                    var emailResult = await _emailService.SendEmailAsync(email, "Reset your password", htmlContent, resetUrl);
                    if (!emailResult.Success)
                    {
                        throw new Exception(string.Join(',', emailResult.Errors.Select(x => x.Message)));
                        
                    }
                    return RedirectToAction("ResetSent");
                }
            }
            ModelState.AddModelError("email", "Email is not valid");
            return View();
        }


        public IActionResult ResetSent()
        {
            return View();
        }

        public IActionResult ResetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string userId, string password)
        {
            var user = await _signInManager.UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.UserManager.ResetPasswordAsync(user, id, password);
                return RedirectToAction("SignIn");
            }
            return BadRequest();
        }

        public async Task<IActionResult> Confirm(string id, string userId)
        {
            var user = await _signInManager.UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await _signInManager.UserManager.ConfirmEmailAsync(user, id);
                return RedirectToAction("Index", "Home");
            }
            return BadRequest();


        }
    }
}