using LeaveManagementT5.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementT5.Controllers
{
    public class ApplicationUserController : Controller
    {
        private AppDbContext _context;

        public ApplicationUserController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            ViewData["Message"] = "Get all users from database";
            return View(_context.Users.ToList());
        }
    }
}
