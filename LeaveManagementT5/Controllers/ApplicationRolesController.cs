using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LeaveManagementT5.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class ApplicationRolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationRolesController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        //List all the roles created by users
        public IActionResult Index()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(IdentityRole model)
        {
            
            if (!await _roleManager.RoleExistsAsync(model.Name))
            {
                
                var newRole = new IdentityRole
                {
                    Name = model.Name
                };
                var result = await _roleManager.CreateAsync(newRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    
                    ModelState.AddModelError(string.Empty, "The Role Could Not Be Created.");
                }
            }
           

            return View(model);
        }


    }
}
