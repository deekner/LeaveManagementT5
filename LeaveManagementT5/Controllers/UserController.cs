using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using LeaveManagementT5.Models;
using LeaveManagementT5.Data;
using Microsoft.AspNetCore.Authorization;

namespace LeaveManagementT5.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
       

        public UserController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DisplayUsers()
        {
           
           var users = _userManager.Users
           .Select(u => new UserViewModel
           {
                
             Id = u.Id,
             UserName = u.UserName,
             //Email = u.Email, //Email är exakt samma som username, känns inte nödvändgit att ha med det två gånger
             PhoneNumber = u.PhoneNumber,
             FirstName = (string)u.GetType().GetProperty("FirstName").GetValue(u, null),
             LastName = (string)u.GetType().GetProperty("LastName").GetValue(u, null),
                
           }).ToList();

           return View("DisplayUser", users);
            
        }



    }
}
