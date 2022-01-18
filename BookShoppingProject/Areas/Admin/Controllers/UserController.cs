using BookShoppingProject.DataAccess.Data;
using BookShoppingProject.Model;
using BookShoppingProject.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookShoppingProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_User_Admin +","+ SD.Role_User_Employee)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _context.ApplicationUsers.Include(u => u.Company).ToList();
            var userRole = _context.UserRoles.ToList();
            var roles = _context.Roles.ToList();

            foreach(var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                if(user.Company==null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            // to not display the admin user to another user
            if(!User.IsInRole(SD.Role_User_Admin))
            {
                var userAdmin = userList.FirstOrDefault(u => u.Role == SD.Role_User_Admin);
                userList.Remove(userAdmin);
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            bool islocked = false;
            var userInDb = _context.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (userInDb == null)
                return Json(new { success = false, message = "Error While Locking and Unlocking User" });
            if(userInDb!=null && userInDb.LockoutEnd>DateTime.Now)
            {
                userInDb.LockoutEnd = DateTime.Now;
                islocked = false;
            }
            else
            {
                userInDb.LockoutEnd = DateTime.Now.AddYears(50);
                islocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = islocked == true ? "User Successfully Locked" : "User Successfully Unlocked" });
        }
        #endregion
    }
}
