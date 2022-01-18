using BookShoppingProject.DataAccess.Repository.IRepository;
using BookShoppingProject.Model;
using BookShoppingProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShoppingProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_User_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int?id)
        {
            Category category = new Category();
            if (id == null)
                return View(category);
            var categoryInDb = _unitOfWork.category.Get(id.GetValueOrDefault());
            return View(categoryInDb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category category)
        {
            if (category == null)
                return NotFound();
            if (!ModelState.IsValid)
                return View(category);
            if (category.Id == 0)
                _unitOfWork.category.Add(category);
            else
                _unitOfWork.category.Update(category);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }


        #region API CALLs
        [HttpGet]
        public IActionResult GetAll()
        {
            var categoryList = _unitOfWork.category.GetAll();
            return Json(new { data = categoryList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (id == 0)
                return NotFound();
            var categoryInDb = _unitOfWork.category.Get(id);
            if (categoryInDb == null)
                return Json(new { success = false, message = "Error while Deleting record" });
            _unitOfWork.category.Remove(categoryInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Record Deleted Successfully" });
        }
        #endregion
    }
}
