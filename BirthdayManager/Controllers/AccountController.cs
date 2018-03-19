using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BirthdayManager.Core.Constants;
using BirthdayManager.Core.Enums;
using BirthdayManager.Core.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using BirthdayManager.Models;
using BirthdayManager.Persistence;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BirthdayManager.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        [AllowAnonymous]
        public async Task Setup()
        {
            var usersSeed = new List<ApplicationUser>();
            var roleStore = new RoleStore<IdentityRole>(new ApplicationDbContext());
            var roleManager = new RoleManager<IdentityRole>(roleStore);

            #region users

            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Victor",
                LastName = "Angheluta",
                MonthOfBirth = 5,
                DayOfBirth = 11,
                Email = "Victor.Angheluta@endava.com",
                UserName = "VANGHELUTA",
                Location = Location.Tower
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Diana",
                LastName = "Arsenii",
                MonthOfBirth = 12,
                DayOfBirth = 15,
                Email = "Diana.Arsenii@endava.com",
                UserName = "DARSENII",
                Location = Location.Tower
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Serghei",
                LastName = "Avdonin",
                MonthOfBirth = 6,
                DayOfBirth = 1,
                Location = Location.Tower,
                UserName = "SAVDONIN",
                Email = "Serghei.Avdonin@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Daniela",
                LastName = "Baciu",
                MonthOfBirth = 7,
                DayOfBirth = 11,
                Location = Location.NBC,
                UserName = "DBACIU",
                Email = "Daniela.Baciu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Simion",
                LastName = "Balan",
                MonthOfBirth = 6,
                DayOfBirth = 15,
                Location = Location.NBC,
                UserName = "SBALAN",
                Email = "Simion.Balan@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Igor",
                LastName = "Bannicov",
                MonthOfBirth = 6,
                DayOfBirth = 12,
                Location = Location.NBC,
                UserName = "IBANNICOV",
                Email = "Igor.Bannicov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Mihail",
                LastName = "Beregoi",
                MonthOfBirth = 11,
                DayOfBirth = 19,
                Location = Location.Tower,
                UserName = "MBEREGOI",
                Email = "Mihail.Beregoi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Sevastian",
                LastName = "Odobescu",
                MonthOfBirth = 3,
                DayOfBirth = 5,
                Location = Location.NBC,
                UserName = "sodobescu",
                Email = "Sevastian.Odobescu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Andrian",
                LastName = "Blidaru",
                MonthOfBirth = 9,
                DayOfBirth = 7,
                Location = Location.NBC,
                UserName = "ABLIDARU",
                Email = "Andrian.Blidaru@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Stanislav",
                LastName = "Bogdanschi",
                MonthOfBirth = 1,
                DayOfBirth = 10,
                Location = Location.NBC,
                UserName = "SBOGDANSCHI",
                Email = "Stanislav.Bogdanschi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandru",
                LastName = "Hangan",
                MonthOfBirth = 3,
                DayOfBirth = 8,
                Location = Location.NBC,
                UserName = "AHANGAN",
                Email = "Alexandru.Hangan@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandru",
                LastName = "Bosii",
                MonthOfBirth = 6,
                DayOfBirth = 4,
                Location = Location.NBC,
                UserName = "ABOSII",
                Email = "Alexandru.Hangan@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ina",
                LastName = "Botnari",
                MonthOfBirth = 12,
                DayOfBirth = 2,
                Location = Location.Tower,
                UserName = "IBOTNRI",
                Email = "Ina.Botnari@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Eugen",
                LastName = "Buga",
                MonthOfBirth = 2,
                DayOfBirth = 3,
                Location = Location.Tower,
                UserName = "EBUGA",
                Email = "Eugen.Buga@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Adrian",
                LastName = "Bunici",
                MonthOfBirth = 6,
                DayOfBirth = 24,
                Location = Location.Tower,
                UserName = "ABUNICI",
                Email = "Adrian.Bunici@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Anastasia",
                LastName = "Buzmacova",
                MonthOfBirth = 11,
                DayOfBirth = 9,
                Location = Location.NBC,
                UserName = "ABUZMACOVA",
                Email = "Anastasia.Buzmacova@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Anatolie",
                LastName = "Canciuc",
                MonthOfBirth = 9,
                DayOfBirth = 26,
                Location = Location.NBC,
                UserName = "ACANCIUC",
                Email = "Anatolie.Canciuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ilia",
                LastName = "Cebotari",
                MonthOfBirth = 8,
                DayOfBirth = 2,
                Location = Location.NBC,
                UserName = "ICEBOTARI",
                Email = "Ilia.Cebotari@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Vasile",
                LastName = "Celac",
                MonthOfBirth = 9,
                DayOfBirth = 16,
                Location = Location.NBC,
                UserName = "VCELAC",
                Email = "Vasile.Celac@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Mihail",
                LastName = "Cepoi",
                MonthOfBirth = 2,
                DayOfBirth = 25,
                Location = Location.NBC,
                UserName = "VCELAC",
                Email = "Vasile.Celac@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Victoria",
                LastName = "Cerbu",
                MonthOfBirth = 2,
                DayOfBirth = 25,
                Location = Location.NBC,
                UserName = "VCERBU",
                Email = "Victoria.Cerbu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Tatiana",
                LastName = "Chitina",
                MonthOfBirth = 11,
                DayOfBirth = 15,
                Location = Location.NBC,
                UserName = "TCHITINA",
                Email = "Tatiana.Chitina@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Adrian",
                LastName = "Cobilas",
                MonthOfBirth = 10,
                DayOfBirth = 5,
                Location = Location.NBC,
                UserName = "ACOBILAS",
                Email = "Adrian.Cobilas@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Radu",
                LastName = "Cojocaru",
                MonthOfBirth = 2,
                DayOfBirth = 17,
                Location = Location.NBC,
                UserName = "RACOJOCARU",
                Email = "Radu.Cojocaru@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Anastasia",
                LastName = "Corotcova",
                MonthOfBirth = 1,
                DayOfBirth = 11,
                Location = Location.NBC,
                UserName = "ACOROTCOVA",
                Email = "Anastasia.Corotcova@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ana",
                LastName = "Costin",
                MonthOfBirth = 5,
                DayOfBirth = 11,
                Location = Location.NBC,
                UserName = "ACOSTIN",
                Email = "Ana.Costin@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Marin",
                LastName = "Efros",
                MonthOfBirth = 3,
                DayOfBirth = 14,
                Location = Location.NBC,
                UserName = "MEFROS",
                Email = "Marin.Efros@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Petru",
                LastName = "Covaliov",
                MonthOfBirth = 8,
                DayOfBirth = 24,
                Location = Location.Tower,
                UserName = "PCOVALIOV",
                Email = "Petru.Covaliov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Anastasia",
                LastName = "Culacov",
                MonthOfBirth = 6,
                DayOfBirth = 26,
                Location = Location.Tower,
                UserName = "ACULACOV",
                Email = "Anastasia.Culacov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandr",
                LastName = "Cusnir",
                MonthOfBirth = 9,
                DayOfBirth = 5,
                Location = Location.Tower,
                UserName = "ACUSNIR",
                Email = "Alexandr.Cusnir@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Dmitrii",
                LastName = "Cuznetchii",
                MonthOfBirth = 10,
                DayOfBirth = 22,
                Location = Location.NBC,
                UserName = "dkuznetski",
                Email = "Dmitrii.Cuznetchii@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Elena",
                LastName = "Iliina",
                MonthOfBirth = 3,
                DayOfBirth = 16,
                Location = Location.NBC,
                UserName = "eiliina",
                Email = "Elena.Iliina@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Artur",
                LastName = "Dobrea",
                MonthOfBirth = 5,
                DayOfBirth = 15,
                Location = Location.NBC,
                UserName = "adobrya",
                Email = "Artur.Dobrea@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandru",
                LastName = "Dogaru",
                MonthOfBirth = 11,
                DayOfBirth = 12,
                Location = Location.Tower,
                UserName = "adogarlu",
                Email = "Alexandru.Dogaru@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Stela",
                LastName = "Cotaga",
                MonthOfBirth = 3,
                DayOfBirth = 16,
                Location = Location.NBC,
                UserName = "stotaga",
                Email = "Stela.Cotaga@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ina",
                LastName = "Don",
                MonthOfBirth = 11,
                DayOfBirth = 10,
                Location = Location.NBC,
                UserName = "idon",
                Email = "Ina.Don@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Oxana",
                LastName = "Donciu",
                MonthOfBirth = 1,
                DayOfBirth = 30,
                Location = Location.NBC,
                UserName = "oxdonciu",
                Email = "Oxana.Donciu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Sergiu",
                LastName = "Drahnea",
                MonthOfBirth = 1,
                DayOfBirth = 4,
                Location = Location.Tower,
                UserName = "SDRAHNEA",
                Email = "Sergiu.Drahnea@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Andrei",
                LastName = "Drumov",
                MonthOfBirth = 7,
                DayOfBirth = 29,
                Location = Location.NBC,
                UserName = "ADRUMOV",
                Email = "Andrei.Drumov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Artur",
                LastName = "Durlesteanu",
                MonthOfBirth = 9,
                DayOfBirth = 21,
                Location = Location.NBC,
                UserName = "adurleasteanu",
                Email = "Artur.Durlesteanu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Rinat",
                LastName = "Papuc",
                MonthOfBirth = 3,
                DayOfBirth = 18,
                Location = Location.NBC,
                UserName = "RPAPUC",
                Email = "Rinat.Papuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Iulia",
                LastName = "Eremeeva",
                MonthOfBirth = 7,
                DayOfBirth = 12,
                Location = Location.NBC,
                UserName = "IEREMEEVA",
                Email = "Iulia.Eremeeva@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexei",
                LastName = "Fortuna",
                MonthOfBirth = 10,
                DayOfBirth = 1,
                Location = Location.Tower,
                UserName = "AFORTUNA",
                Email = "Alexei.Fortuna@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Serghei",
                LastName = "Golovatii",
                MonthOfBirth = 1,
                DayOfBirth = 26,
                Location = Location.Tower,
                UserName = "SGOLOVATII",
                Email = "Serghei.Golovatii@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Liuba",
                LastName = "Gonta",
                MonthOfBirth = 5,
                DayOfBirth = 5,
                Location = Location.NBC,
                UserName = "LGONTA",
                Email = "Liuba.Gonta@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Gorgos",
                MonthOfBirth = 1,
                DayOfBirth = 1,
                Location = Location.NBC,
                UserName = "IGORGOS",
                Email = "Ion.Gorgos@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Sergiu",
                LastName = "Dominic",
                MonthOfBirth = 3,
                DayOfBirth = 21,
                Location = Location.NBC,
                UserName = "SDOMINIC",
                Email = "Sergiu.Dominic@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandr",
                LastName = "Gumeniuc",
                MonthOfBirth = 5,
                DayOfBirth = 6,
                Location = Location.Tower,
                UserName = "AGUMENIUC",
                Email = "Alexandr.Gumeniuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Dumitru",
                LastName = "Gurjui",
                MonthOfBirth = 11,
                DayOfBirth = 6,
                Location = Location.Tower,
                UserName = "DGURJUI",
                Email = "Dumitru.Gurjui@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexei",
                LastName = "Gutaga",
                MonthOfBirth = 5,
                DayOfBirth = 10,
                Location = Location.NBC,
                UserName = "AGUTAGA",
                Email = "Alexei.Gutaga@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Egor",
                LastName = "Guzun",
                MonthOfBirth = 6,
                DayOfBirth = 22,
                Location = Location.NBC,
                UserName = "EGUZUN",
                Email = "Egor.Guzun@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexei",
                LastName = "Borisco",
                MonthOfBirth = 6,
                DayOfBirth = 22,
                Location = Location.Tower,
                UserName = "ABORISCO",
                Email = "Alexei.Borisco@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexei",
                LastName = "Borisco",
                MonthOfBirth = 3,
                DayOfBirth = 21,
                Location = Location.Tower,
                UserName = "ABORISCO",
                Email = "Alexei.Borisco@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Harin",
                MonthOfBirth = 2,
                DayOfBirth = 28,
                Location = Location.NBC,
                UserName = "IHARIN",
                Email = "Ion.Harin@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Maxim",
                LastName = "Hristiniuc",
                MonthOfBirth = 8,
                DayOfBirth = 19,
                Location = Location.NBC,
                UserName = "MHristiniuc",
                Email = "Maxim.Hristiniuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandru",
                LastName = "Iachimov",
                MonthOfBirth = 2,
                DayOfBirth = 23,
                Location = Location.Tower,
                UserName = "AIACHIMOV",
                Email = "Alexandru.Iachimov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Dmitrii",
                LastName = "Iascov",
                MonthOfBirth = 10,
                DayOfBirth = 31,
                Location = Location.Tower,
                UserName = "DIascov",
                Email = "Dmitrii.Iascov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Iulia",
                LastName = "Mazilu",
                MonthOfBirth = 4,
                DayOfBirth = 3,
                Location = Location.NBC,
                UserName = "IMAZILU",
                Email = "Iulia.Mazilu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Eduard",
                LastName = "Laur",
                MonthOfBirth = 5,
                DayOfBirth = 18,
                Location = Location.Tower,
                UserName = "ELAUR",
                Email = "Eduard.Laur@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandru",
                LastName = "Lazari",
                MonthOfBirth = 8,
                DayOfBirth = 18,
                Location = Location.NBC,
                UserName = "ELAUR",
                Email = "Eduard.Laur@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Iulia",
                LastName = "Lazo",
                MonthOfBirth = 12,
                DayOfBirth = 21,
                Location = Location.NBC,
                UserName = "ILAZO",
                Email = "Iulia.Lazo@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Luminita",
                LastName = "Leahu",
                MonthOfBirth = 10,
                DayOfBirth = 22,
                Location = Location.NBC,
                UserName = "LLEAHU",
                Email = "Luminita.Leahu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Lica",
                MonthOfBirth = 2,
                DayOfBirth = 22,
                Location = Location.NBC,
                UserName = "ILICA",
                Email = "Ion.Lica@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Lungu",
                MonthOfBirth = 12,
                DayOfBirth = 23,
                Location = Location.NBC,
                UserName = "ILUNGU",
                Email = "Ion.Lungu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Iulia",
                LastName = "Muraseva",
                MonthOfBirth = 4,
                DayOfBirth = 4,
                Location = Location.NBC,
                UserName = "IMURASEVA",
                Email = "Iulia.Muraseva@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Maximciuc",
                MonthOfBirth = 8,
                DayOfBirth = 4,
                Location = Location.Tower,
                UserName = "IMAXIMCIUC",
                Email = "Ion.Maximciuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Roman",
                LastName = "Para",
                MonthOfBirth = 4,
                DayOfBirth = 6,
                Location = Location.NBC,
                UserName = "RPARA",
                Email = "Roman.Para@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ruslan",
                LastName = "Melnic",
                MonthOfBirth = 5,
                DayOfBirth = 24,
                Location = Location.NBC,
                UserName = "RMELNIC",
                Email = "Ruslan.Melnic@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Serghei",
                LastName = "Morari",
                MonthOfBirth = 8,
                DayOfBirth = 17,
                Location = Location.NBC,
                UserName = "SMORARI",
                Email = "Serghei.Morari@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Vasile",
                LastName = "Dmitruc",
                MonthOfBirth = 4,
                DayOfBirth = 7,
                Location = Location.NBC,
                UserName = "VDMITRUC",
                Email = "Vasile.Dmitruc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Vladislav",
                LastName = "Guleaev",
                MonthOfBirth = 4,
                DayOfBirth = 8,
                Location = Location.Tower,
                UserName = "VGULEAEV",
                Email = "Vladislav.Guleaev@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Oxana",
                LastName = "Nanu",
                MonthOfBirth = 8,
                DayOfBirth = 23,
                Location = Location.NBC,
                UserName = "ONANU",
                Email = "Oxana.Nanu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ludmila",
                LastName = "Bezaliuc",
                MonthOfBirth = 4,
                DayOfBirth = 13,
                Location = Location.Tower,
                UserName = "LBEZALIUC",
                Email = "Mila.Bezaliuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alla",
                LastName = "Onoi",
                MonthOfBirth = 11,
                DayOfBirth = 27,
                Location = Location.NBC,
                UserName = "AONOI",
                Email = "Alla.Onoi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Leon",
                LastName = "Osipov",
                MonthOfBirth = 9,
                DayOfBirth = 17,
                Location = Location.Tower,
                UserName = "LOSIPOV",
                Email = "Leon.Osipov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Corneliu",
                LastName = "Pacalev",
                MonthOfBirth = 2,
                DayOfBirth = 28,
                Location = Location.NBC,
                UserName = "CPACALEV",
                Email = "Corneliu.Pacalev@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Radu",
                LastName = "Palamari",
                MonthOfBirth = 1,
                DayOfBirth = 7,
                Location = Location.NBC,
                UserName = "RPALAMARI",
                Email = "Radu.Palamari@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Mihail",
                LastName = "Pankov",
                MonthOfBirth = 10,
                DayOfBirth = 23,
                Location = Location.Tower,
                UserName = "MPANKOV",
                Email = "Mihail.Pankov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandru",
                LastName = "Papuc",
                MonthOfBirth = 8,
                DayOfBirth = 27,
                Location = Location.NBC,
                UserName = "APAPUC",
                Email = "Alexandru.Papuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Eugen",
                LastName = "Papuc",
                MonthOfBirth = 7,
                DayOfBirth = 20,
                Location = Location.NBC,
                UserName = "EPAPUC",
                Email = "Eugen.Papuc@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Maxim",
                LastName = "Tacu",
                MonthOfBirth = 4,
                DayOfBirth = 24,
                Location = Location.NBC,
                UserName = "MTACU",
                Email = "Maxim.Tacu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Tatiana",
                LastName = "Moscalu",
                MonthOfBirth = 4,
                DayOfBirth = 26,
                Location = Location.NBC,
                UserName = "TMOSCALU",
                Email = "Tatiana.Moscalu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Elvira",
                LastName = "Parpalac",
                MonthOfBirth = 10,
                DayOfBirth = 31,
                Location = Location.NBC,
                UserName = "EPARPALAC",
                Email = "Elvira.Parpalac@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Gheorghe",
                LastName = "Pascal",
                MonthOfBirth = 7,
                DayOfBirth = 21,
                Location = Location.NBC,
                UserName = "GHPASCAL",
                Email = "Elvira.Parpalac@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alexandr",
                LastName = "Persin",
                MonthOfBirth = 10,
                DayOfBirth = 30,
                Location = Location.NBC,
                UserName = "APERSIN",
                Email = "Alexandr.Persin@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Vlad",
                LastName = "Picireanu",
                MonthOfBirth = 9,
                DayOfBirth = 8,
                Location = Location.NBC,
                UserName = "VPicireanu",
                Email = "Vlad.Picireanu@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Vladimir",
                LastName = "Placinta",
                MonthOfBirth = 10,
                DayOfBirth = 28,
                Location = Location.NBC,
                UserName = "VPLACINTA",
                Email = "Vladimir.Placinta@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Evgheni",
                LastName = "Popodneac",
                MonthOfBirth = 6,
                DayOfBirth = 18,
                Location = Location.NBC,
                UserName = "EPOPODNEAC",
                Email = "Evgheni.Popodneac@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Iuliana",
                LastName = "Popovici",
                MonthOfBirth = 7,
                DayOfBirth = 4,
                Location = Location.NBC,
                UserName = "IUPOPOVICI",
                Email = "Iuliana.Popovici@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Didina",
                LastName = "Prodan",
                MonthOfBirth = 11,
                DayOfBirth = 23,
                Location = Location.NBC,
                UserName = "DPRODAN",
                Email = "Didina.Prodan@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Rinat",
                LastName = "Pulcinschi",
                MonthOfBirth = 8,
                DayOfBirth = 8,
                Location = Location.Tower,
                UserName = "rpulcinschi",
                Email = "Rinat.Pulcinschi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ecaterina",
                LastName = "Raducan",
                MonthOfBirth = 8,
                DayOfBirth = 26,
                Location = Location.Tower,
                UserName = "ERADUCAN",
                Email = "Ecaterina.Raducan@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Serghei",
                LastName = "Reulet",
                MonthOfBirth = 11,
                DayOfBirth = 26,
                Location = Location.NBC,
                UserName = "SREULET",
                Email = "Serghei.Reulet@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Tatiana",
                LastName = "Reulet",
                MonthOfBirth = 12,
                DayOfBirth = 24,
                Location = Location.NBC,
                UserName = "TREULET",
                Email = "Tatiana.Reulet@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Serghei",
                LastName = "Roman",
                MonthOfBirth = 11,
                DayOfBirth = 26,
                Location = Location.NBC,
                UserName = "SROMAN",
                Email = "Serghei.Roman@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Rosca",
                MonthOfBirth = 10,
                DayOfBirth = 22,
                Location = Location.NBC,
                UserName = "IROSCA",
                Email = "Ion.Rosca@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Veronica",
                LastName = "Rosca",
                MonthOfBirth = 8,
                DayOfBirth = 7,
                Location = Location.NBC,
                UserName = "VROSCA",
                Email = "Veronica.Rosca@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Irina",
                LastName = "Rudenco",
                MonthOfBirth = 10,
                DayOfBirth = 15,
                Location = Location.NBC,
                UserName = "IRUDENCO",
                Email = "Irina.Rudenco@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Serghei",
                LastName = "Rudicov",
                MonthOfBirth = 8,
                DayOfBirth = 26,
                Location = Location.Tower,
                UserName = "SERUDICOV",
                Email = "Serghei.Rudicov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Sandul",
                MonthOfBirth = 5,
                DayOfBirth = 22,
                Location = Location.NBC,
                UserName = "ISANDUL",
                Email = "Ion.Sandul@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Linda",
                LastName = "Serda-Vanghelii (Cozic)",
                MonthOfBirth = 1,
                DayOfBirth = 2,
                Location = Location.NBC,
                UserName = "LCOZIC",
                Email = "Linda.Cozic@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Valeriu",
                LastName = "Sinelnicov",
                MonthOfBirth = 8,
                DayOfBirth = 3,
                Location = Location.Tower,
                UserName = "VSinelnicov",
                Email = "Valeriu.Sinelnicov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ivan",
                LastName = "Sirosenco",
                MonthOfBirth = 1,
                DayOfBirth = 17,
                Location = Location.NBC,
                UserName = "ISirosenco",
                Email = "Ivan.Sirosenco@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Eugeniu",
                LastName = "Smesnoi",
                MonthOfBirth = 5,
                DayOfBirth = 31,
                Location = Location.NBC,
                UserName = "ESmesnoi",
                Email = "Eugeniu.Smesnoi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Eugeniu",
                LastName = "Smesnoi",
                MonthOfBirth = 5,
                DayOfBirth = 31,
                Location = Location.NBC,
                UserName = "ESmesnoi",
                Email = "Eugeniu.Smesnoi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Sergiu",
                LastName = "Speian",
                MonthOfBirth = 7,
                DayOfBirth = 14,
                Location = Location.NBC,
                UserName = "SSPEIAN",
                Email = "Sergiu.Speian@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Nicolae",
                LastName = "Stropsa",
                MonthOfBirth = 8,
                DayOfBirth = 19,
                Location = Location.NBC,
                UserName = "NStropsa",
                Email = "Nicolae.Stropsa@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Alina",
                LastName = "Mafteor",
                MonthOfBirth = 4,
                DayOfBirth = 29,
                Location = Location.NBC,
                UserName = "AMAFTEOR",
                Email = "Alina.Mafteor@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Dmitri",
                LastName = "Telinov",
                MonthOfBirth = 9,
                DayOfBirth = 24,
                Location = Location.NBC,
                UserName = "DTELINOV",
                Email = "Dmitri.Telinov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Serghei",
                LastName = "Tibulschi",
                MonthOfBirth = 1,
                DayOfBirth = 24,
                Location = Location.Tower,
                UserName = "STIBULSCHII",
                Email = "Serghei.Tibulschi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Tomsa",
                MonthOfBirth = 2,
                DayOfBirth = 9,
                Location = Location.NBC,
                UserName = "ITOMSA",
                Email = "Ion.Tomsa@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Irina",
                LastName = "Trofimova",
                MonthOfBirth = 1,
                DayOfBirth = 11,
                Location = Location.NBC,
                UserName = "ITROFIMOVA",
                Email = "Irina.Trofimova@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Roman",
                LastName = "Tudvases",
                MonthOfBirth = 7,
                DayOfBirth = 8,
                Location = Location.Tower,
                UserName = "RTUDVASEV",
                Email = "Roman.Tudvases@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Daniil",
                LastName = "Turcan",
                MonthOfBirth = 8,
                DayOfBirth = 22,
                Location = Location.NBC,
                UserName = "DATURCAN",
                Email = "Daniil.Turcan@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Sergiu",
                LastName = "Ursachi",
                MonthOfBirth = 8,
                DayOfBirth = 10,
                Location = Location.NBC,
                UserName = "SURSACHI",
                Email = "Sergiu.Ursachi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Maxim",
                LastName = "Ustimov",
                MonthOfBirth = 7,
                DayOfBirth = 9,
                Location = Location.NBC,
                UserName = "MUSTIMOV",
                Email = "Maxim.Ustimov@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ecaterina",
                LastName = "Vasilean (Levitchi)",
                MonthOfBirth = 7,
                DayOfBirth = 26,
                Location = Location.Tower,
                UserName = "ELEVITCHI",
                Email = "Ecaterina.Levitchi@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Ion",
                LastName = "Verdes",
                MonthOfBirth = 9,
                DayOfBirth = 30,
                Location = Location.NBC,
                UserName = "EVERDES",
                Email = "Ion.Verdes@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Vlastislava",
                LastName = "Vihrev",
                MonthOfBirth = 10,
                DayOfBirth = 2,
                Location = Location.NBC,
                UserName = "VVlastislava",
                Email = "Vlastislava.Vihrev@endava.com"
            });
            usersSeed.Add(new ApplicationUser()
            {
                FirstName = "Sebastian",
                LastName = "Zavadschi",
                MonthOfBirth = 11,
                DayOfBirth = 21,
                Location = Location.NBC,
                UserName = "SZavadschi",
                Email = "Sebastian.Zavadschi@endava.com"
            });

            #endregion
            foreach (var applicationUser in usersSeed)
            {
                await UserManager.CreateAsync(applicationUser, "Qwerty123456");

                if (applicationUser.UserName == "VGULEAEV" ||
                    applicationUser.UserName == "DPRODAN" ||
                    applicationUser.UserName == "ELEVITCHI" ||
                    applicationUser.UserName == "ILAZO")
                {
                    await roleManager.CreateAsync(new IdentityRole(RoleNames.Admin));
                    await UserManager.AddToRoleAsync(applicationUser.Id, RoleNames.Admin);
                }
            }
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}