using BookShoppingProject.DataAccess.Repository.IRepository;
using BookShoppingProject.Model;
using BookShoppingProject.Model.ViewModels;
using BookShoppingProject.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BookShoppingProject.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        public CartController(IUnitOfWork unitOfWork,UserManager<IdentityUser> userManager,IEmailSender emailSender)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claim==null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                 ListCart=new List<ShoppingCart>()
                };
            }
            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            ShoppingCartVM.OrderHeader.OrderTotal = 0;
            //access user detail and setting in OrderHeader
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstorDefault(u => u.Id == claim.Value, includeProperties: "Company");

            foreach(var list in ShoppingCartVM.ListCart)
            {
                // we have different prices for the same product,so which price should display/use...we have to create a method for this in SD.cs
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Count * list.Price);
                list.Product.Discription = SD.ConvertToRawHtml(list.Product.Discription);
                if(list.Product.Discription.Length>100)
                {
                    list.Product.Discription = list.Product.Discription.Substring(0, 99) + "...";
                }
            }

            return View(ShoppingCartVM);
        }
        public IActionResult Plus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.FirstorDefault(sc => sc.Id == cartId, includeProperties: "Product");
            cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult minus(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.FirstorDefault(sc => sc.Id == cartId, includeProperties: "Product");
            cart.Count -= 1;
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult remove(int cartId)
        {
            var cart = _unitOfWork.ShoppingCart.FirstorDefault(sc => sc.Id == cartId, includeProperties: "Product");
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();

            var count = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cart.ApplicationUserId, includeProperties: "Product").ToList().Count;
            HttpContext.Session.SetInt32(SD.ssShoppingcartSession, count);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Summary()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ShoppingCartVM = new ShoppingCartVM()
            {
                OrderHeader = new OrderHeader(),
                ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product")
            };

            //for user detail
            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstorDefault(u => u.Id == claim.Value, includeProperties: "Company");
            
            //for price
            foreach(var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.OrderHeader.OrderTotal += (list.Count * list.Price);
                list.Product.Discription = SD.ConvertToRawHtml(list.Product.Discription);
            }

            // setting details of user in OrderHeader
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost(string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            //to set details of user in OrderHeader

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstorDefault(u => u.Id == claim.Value, includeProperties: "Company");

            //jis user ne login kara uska sara data in ListCart
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == claim.Value, includeProperties: "Product");

            //creating orderstatus and Payment status in BookShoppingProject.Utility=>SD.cs
            //then come back here

            ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = claim.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();


            foreach(var item in ShoppingCartVM.ListCart)
            {
                item.Price = SD.GetPriceBasedOnQuantity(item.Count, item.Product.Price, item.Product.Price50, item.Product.Price100);

                OrderDetail orderDetail = new OrderDetail()
                { 
                 ProductId=item.ProductId,
                 OrderId=ShoppingCartVM.OrderHeader.Id,
                 Price=item.Price,
                 Count=item.Count
                };

                ShoppingCartVM.OrderHeader.OrderTotal += orderDetail.Price * orderDetail.Count;
                _unitOfWork.OrderDetail.Add(orderDetail);
            }

            _unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);
            _unitOfWork.Save();

            HttpContext.Session.SetInt32(SD.ssShoppingcartSession, 0);

            //Stripe Payment

            #region Stripe Payment
            if(stripeToken==null)//for company login
            {
                ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.PaymentStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions
                {
                    Amount=Convert.ToInt32(ShoppingCartVM.OrderHeader.OrderTotal),
                    Currency="inr",
                    Description="OrderId :"+ShoppingCartVM.OrderHeader.Id,
                    Source=stripeToken
                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                if (charge.BalanceTransactionId == null)
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    ShoppingCartVM.OrderHeader.TransactionId = charge.BalanceTransactionId;

                if(charge.Status.ToLower()=="succeeded")
                {
                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                    ShoppingCartVM.OrderHeader.PaymentDate = DateTime.Now;
                }
                _unitOfWork.Save();

            }

            #endregion

            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int Id)
        {
            return View(Id);
        }
    }
}
