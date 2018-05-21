using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                IdentityResult creationResult =await this._signInManager.UserManager.CreateAsync(newUser);

                if (creationResult.Succeeded)
                {
                    //TODO: Create an account and log this user in
                    IdentityResult passwordResult = await this._signInManager.UserManager.AddPasswordAsync(newUser, model.Password);
                    if (passwordResult.Succeeded)
                    {
                        #region use the SendGrid client to send a welcome email
                        var emailResult = await this._emailService.SendEmailAsync(
                        model.Email,
                        "Welcome to BikeStore!",
                        "Thanks for signing up, " + model.UserName + "!",
                        "<p>Thanks for signing up, " + model.UserName + "!</p>"
                    );
                        if (emailResult.Success)
                            return RedirectToAction("Index", "Home");
                        else
                            return BadRequest(emailResult.Message);
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
    }
}