using BookShoppingProject.DataAccess.Repository.IRepository;
using BookShoppingProject.Model;
using BookShoppingProject.Model.ViewModels;
using BookShoppingProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookShoppingProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWork ,ILogger<HomeController> logger)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim!=null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32(SD.ssShoppingcartSession,count);
            }
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return View(productList);
        }
        public IActionResult Details(int id)
        {
            var productInDb = _unitOfWork.Product.FirstorDefault(p => p.Id == id, includeProperties: "Category,CoverType");
            var shoppingCart = new ShoppingCart()
            { 
              Product=productInDb,
              ProductId=productInDb.Id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCartObj)
        {
            shoppingCartObj.Id = 0;
            if(ModelState.IsValid)
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCartObj.ApplicationUserId = claim.Value;
                //increase in count of quantity
                var shoppingCartFromDb = _unitOfWork.ShoppingCart.FirstorDefault(u => u.ApplicationUserId == shoppingCartObj.ApplicationUserId && u.ProductId == shoppingCartObj.ProductId, includeProperties: "Product");
                if (shoppingCartFromDb == null)
                    _unitOfWork.ShoppingCart.Add(shoppingCartObj);
                else
                    shoppingCartFromDb.Count += shoppingCartObj.Count;
                _unitOfWork.Save();

                // session to display itemcount in cart
                var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value).ToList().Count;

                //to store count in session
                HttpContext.Session.SetInt32(SD.ssShoppingcartSession, count);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var productInDb = _unitOfWork.Product.FirstorDefault(p => p.Id == shoppingCartObj.Id, includeProperties: "Category,CoverType");
                var shoppingCart = new ShoppingCart()
                {
                    Product = productInDb,
                    ProductId = productInDb.Id
                };
                return View(shoppingCart);
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
