using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BookShoppingProject.DataAccess.Repository.IRepository;
using BookShoppingProject.Model;
using BookShoppingProject.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BookShoppingProject.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            // Add Custom Columns which we made in ApplicationUser Model
            [Required]
            public string Name { get; set; }
            [Display(Name ="Street Address")]
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            [Display(Name ="Postal Code")]
            public string PostalCode { get; set; }
            [Display(Name ="Phone Number")]
            public string PhoneNumber { get; set; }
            [Display(Name ="Company")]
            public int? CompanyId { get; set; }
            public string Role { get; set; }

            //when admin login then these dropdownlist will display otherwise not
            //company and Roles

            public IEnumerable<SelectListItem> CompanyList { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }


        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            //populating CompanyList and RoleList
            Input = new InputModel()
            { 
             CompanyList=_unitOfWork.Company.GetAll().Select(c=> new SelectListItem() 
             { Text=c.Name,
               Value=c.Id.ToString()
             }),
             RoleList=_roleManager.Roles.Where(m=>m.Name != SD.Role_User_Admin).Select(x=>x.Name).Select(rl=> new SelectListItem()
             {
                 Text=rl,
                 Value=rl
             })

            };
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                // var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var user = new ApplicationUser()
                {
                 Name=Input.Name,
                 Email=Input.Email,
                 UserName=Input.Email,
                 StreetAddress=Input.StreetAddress,
                 State=Input.State,
                 City=Input.City,
                 PhoneNumber=Input.PhoneNumber,
                 PostalCode=Input.PostalCode,
                 CompanyId=Input.CompanyId,
                 Role=Input.Role
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // create role--check if Role exist ,then create new identityRole

                    if(! await _roleManager.RoleExistsAsync(SD.Role_User_Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Admin));
                    }
                    if(!await _roleManager.RoleExistsAsync(SD.Role_User_Company))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Company));
                    }
                    if(!await _roleManager.RoleExistsAsync(SD.Role_User_Employee))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Employee));
                    }
                    if(!await _roleManager.RoleExistsAsync(SD.Role_User_Individual))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Individual));
                    }

                    //Code run only once for the first user -which should be Admin
                    //After that this code will be comment out 

                   // await _userManager.AddToRoleAsync(user, SD.Role_User_Admin);


                    //assigning role to user after they created
                    if(user.Role==null)
                    { 
                        await _userManager.AddToRoleAsync(user, SD.Role_User_Individual);
                    }
                    else
                    {
                        if(user.CompanyId>0)
                        {
                            await _userManager.AddToRoleAsync(user, SD.Role_User_Company);
                        }
                        else
                        {
                            await _userManager.AddToRoleAsync(user, user.Role);
                        }
                    }
                    //commentout email code
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        //after registartion only individual user/ user with no role ,will get auto logged in
                        if(user.Role==null)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return LocalRedirect(returnUrl);
                        }
                        else
                        {
                            //Admin user
                            return RedirectToAction("Index", "User", new { Area = "Admin" });
                        }
                       
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
