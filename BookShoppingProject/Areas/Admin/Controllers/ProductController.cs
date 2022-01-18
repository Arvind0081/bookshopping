using BookShoppingProject.DataAccess.Repository.IRepository;
using BookShoppingProject.Model;
using BookShoppingProject.Model.ViewModels;
using BookShoppingProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BookShoppingProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_User_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.category.GetAll().Select(c => new SelectListItem() {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                CoverTypeList = _unitOfWork.coverType.GetAll().Select(cv => new SelectListItem()
                {
                    Text = cv.Name,
                    Value = cv.Id.ToString()
                })
            };
            if (id == null)
                return View(productVM);
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            return View(productVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM)
        {
            if(ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath;

                var files = HttpContext.Request.Form.Files;
                if(files.Count()>0)
                {
                    //generate random name of file
                    string fileName = Guid.NewGuid().ToString();

                    //to set path where to store image
                    var uploads = Path.Combine(webRootPath, @"images\Products");

                    //to get extension of file
                    var extension = Path.GetExtension(files[0].FileName);

                    if(productVM.Product.Id!=0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }

                    //Incase of Updte To delete old physical file
                    if(productVM.Product.ImageUrl!=null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // to save image in Products folder
                    using(var fileStream=new FileStream(Path.Combine(uploads,fileName+extension),FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }

                    //to save image path in table
                    productVM.Product.ImageUrl = @"\images\Products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                }
                if (productVM.Product.Id == 0)

                    _unitOfWork.Product.Add(productVM.Product);
                else
                    _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
                return RedirectToAction(nameof(Index));

            }
            else
            {
                productVM = new ProductVM()
                {
                    CategoryList = _unitOfWork.category.GetAll().Select(c => new SelectListItem()
                    {
                        Text = c.Name,
                        Value = c.Id.ToString()
                    }),
                    CoverTypeList=_unitOfWork.coverType.GetAll().Select(cv=> new SelectListItem()
                    {
                        Text=cv.Name,
                        Value=cv.Id.ToString()
                    })
                };
               
                if(productVM.Product.Id!=0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                }
                return View(productVM);

            }

            

            
        }

        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category,CoverType");
            return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (id == 0)
                return NotFound();
            var productInDb = _unitOfWork.Product.Get(id);
            if (productInDb == null)
                return Json(new { success = false, message = "Error while Deleting record" });
            _unitOfWork.Product.Remove(productInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Record Deleted Successfully" });
        }
        #endregion
    }
}
