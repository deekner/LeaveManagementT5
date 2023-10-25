using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveManagementT5.Data;
using LeaveManagementT5.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

public class LeaveAllocationController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public LeaveAllocationController(AppDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: LeaveAllocation
    
    public async Task<IActionResult> Index()
    {
        var leaveAllocations = await _context.LeaveAllocation
            .Include(la => la.Employee)
            .Include(la => la.LeaveType)
            .ToListAsync();

        return View(leaveAllocations);
    }

    // GET: LeaveAllocation/Create
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName");
        ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name");
        return View();
    }

    // POST: LeaveAllocation/Create
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LeaveAllocation leaveAllocation)
    {
        if (ModelState.IsValid)
        {
            _context.Add(leaveAllocation);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName", leaveAllocation.EmployeeId);
        ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveAllocation.LeaveTypeId);
        return View(leaveAllocation);
    }

    // GET: LeaveAllocation/Edit/5
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var leaveAllocation = await _context.LeaveAllocation.FindAsync(id);

        if (leaveAllocation == null)
        {
            return NotFound();
        }

        ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName", leaveAllocation.EmployeeId);
        ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveAllocation.LeaveTypeId);

        return View(leaveAllocation);
    }

    // POST: LeaveAllocation/Edit/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LeaveAllocation leaveAllocation)
    {
        if (id != leaveAllocation.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(leaveAllocation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeaveAllocationExists(leaveAllocation.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Index");
        }
        ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName", leaveAllocation.EmployeeId);
        ViewData["LeaveTypeId"] = new SelectList(_context.LeaveTypes, "Id", "Name", leaveAllocation.LeaveTypeId);

        return View(leaveAllocation);
    }

    
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var leaveAllocation = await _context.LeaveAllocation
            .Include(la => la.Employee)
            .Include(la => la.LeaveType)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (leaveAllocation == null)
        {
            return NotFound();
        }

        return View(leaveAllocation);
    }

    // POST: LeaveAllocation/Delete/5
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var leaveAllocation = await _context.LeaveAllocation.FindAsync(id);

        if (leaveAllocation != null)
        {
            _context.LeaveAllocation.Remove(leaveAllocation);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    private bool LeaveAllocationExists(int id)
    {
        return _context.LeaveAllocation.Any(e => e.Id == id);
    }

     [Authorize(Roles = "Employee")]
    public IActionResult EmployeeLeaveAllocations()
    {
        // Retrieve the current user (employee)
        var user = _userManager.GetUserAsync(User).Result;

        

        var employeeLeaveAllocations = _context.LeaveAllocation
            .Include(la => la.LeaveType)
            .Where(alloc => alloc.EmployeeId == user.Id).ToList();


        return View("MyLeaveAllocation", employeeLeaveAllocations);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminLeaveAllocations()
    {
        // Retrieve the current user (employee)
        var user = _userManager.GetUserAsync(User).Result;

        // Retrieve leave allocations for the current employee
        var employeeLeaveAllocations = _context.LeaveAllocation
            .Where(alloc => alloc.EmployeeId == user.Id)
            .ToList();

        return View("Index", employeeLeaveAllocations);
    }

    //Just nu gör denna Controllen inte så mycket. Du kan sätta hur många dagar en Employee ska kunna ha ledigt i kategorier
    //Men det finns inget som kollar och räknar ner dina dagar. Säg att du tar 10 dagar semester. LeaveAllocatin för denna employee bör gå ner från 25 till 15 när den blivit godkänd
}
