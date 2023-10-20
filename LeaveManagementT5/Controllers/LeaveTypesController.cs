
using Microsoft.AspNetCore.Mvc;
using LeaveManagementT5.Data;
using LeaveManagementT5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementT5.Controllers
{
    [Authorize(Roles ="Admin")]
    public class LeaveTypesController : Controller
    {
        private readonly AppDbContext _context;

        public LeaveTypesController(AppDbContext context)
        {
            _context = context;
            
        }


        public IActionResult Index()
        {
            var leaveTypes = _context.LeaveTypes.ToList();
            return View(leaveTypes);
            
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(LeaveType leaveType)
        {
            if (ModelState.IsValid)
            {
                _context.LeaveTypes.Add(leaveType);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(leaveType);
        }

        public IActionResult Edit(int id)
        {
            var leaveType = _context.LeaveTypes.Find(id);
            if (leaveType == null)
            {
                return NotFound();
            }
            return View(leaveType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, LeaveType leaveType)
        {
            if (id != leaveType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(leaveType);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(leaveType);
        }

        public IActionResult Delete(int id)
        {
            var leaveType = _context.LeaveTypes.Find(id);
            if (leaveType == null)
            {
                return NotFound();
            }
            return View(leaveType);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var leaveType = _context.LeaveTypes.Find(id);
            if (leaveType != null)
            {
                _context.LeaveTypes.Remove(leaveType);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.LeaveTypes == null)
            {
                return NotFound();
            }

            var leaveType = await _context.LeaveTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (leaveType == null)
            {
                return NotFound();
            }

            return View(leaveType);
        }
    }
}
