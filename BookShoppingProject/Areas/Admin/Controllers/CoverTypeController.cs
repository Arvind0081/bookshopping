using BookShoppingProject.DataAccess.Repository.IRepository;
using BookShoppingProject.Model;
using BookShoppingProject.Utility;
using Dapper;
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
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null)
                return View(coverType);
            //var param = new DynamicParameters();
            //param.Add("@Id", id);
            //var coverTypeInDb = _unitOfWork.SP_CALLS.OneRecord<CoverType>(SD.GetCoverType,param);
            var coverTypeInDb = _unitOfWork.coverType.Get(id.GetValueOrDefault());
            if (coverTypeInDb == null)
                return NotFound();
            return View(coverTypeInDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CoverType coverType)
        {
            if (coverType == null)
                return NotFound();
            if (!ModelState.IsValid)
                return View(coverType);
            //var param = new DynamicParameters();
            //param.Add("@Name", coverType.Name);
            if (coverType.Id == 0)
              //  _unitOfWork.SP_CALLS.Execute(SD.CreateCoverType, param);
             _unitOfWork.coverType.Add(coverType);
            else
             _unitOfWork.coverType.Update(coverType);
            _unitOfWork.Save();
            //{
            //    param.Add("@Id", coverType.Id);
            //    _unitOfWork.SP_CALLS.Execute(SD.UpdateCoverType,param);

            //}

            

            return RedirectToAction(nameof(Index));
        }
        #region APIs
        public IActionResult GetAll()
        {//using storeProcedure
           // var coverTypeList = _unitOfWork.SP_CALLS.List<CoverType>(SD.GetCoverTypes);
            var coverTypeList = _unitOfWork.coverType.GetAll();
            return Json(new { data = coverTypeList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if (id == 0)
                return NotFound();
              var coverTypeInDb = _unitOfWork.coverType.Get(id);
            //var param = new DynamicParameters();
            //param.Add("@Id", id);
            //var coverTypeInDb = _unitOfWork.SP_CALLS.OneRecord<CoverType>(SD.GetCoverType, param);
            if (coverTypeInDb == null)
                return Json(new { success = false, message = "Error While Deleting record" });
            _unitOfWork.coverType.Remove(coverTypeInDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Record Successfully Deleted" });
        }
        #endregion
    }
}
