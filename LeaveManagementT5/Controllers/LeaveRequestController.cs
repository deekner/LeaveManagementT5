using LeaveManagementT5.Data;
using LeaveManagementT5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class LeaveRequestController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _context;

    public LeaveRequestController(UserManager<IdentityUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Index(string status)
    {
        var query = _context.LeaveRequest
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsQueryable(); // Create a base query to build upon

        // Check if a status filter is provided
        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(lr => lr.Status == status);
        }

        var leaveRequests = query.ToList();

        return View(leaveRequests);
    }

    [Authorize(Roles = "Employee")]
    public IActionResult Create()
    {
        // Load leave types into a dropdown for selection
        var leaveTypes = _context.LeaveTypes.ToList();
        ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name", "DefaultDays");
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Employee")]
    public IActionResult Create(LeaveRequest leaveRequest)
    {
        // Set the status to "Pending" (default)
        leaveRequest.Status = "Pending";

        
        var user = _userManager.GetUserAsync(User).Result;
        leaveRequest.EmployeeId = user.Id;

        //Räkna ut dagarna genom att ta slut minus start 
        int requestedDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days;

        
        var selectedLeaveType = _context.LeaveTypes.FirstOrDefault(lt => lt.Id == leaveRequest.LeaveTypeId);

        if (selectedLeaveType != null && requestedDays <= selectedLeaveType.DefaultDays)
        {
           
            _context.LeaveRequest.Add(leaveRequest);
            _context.SaveChanges();
            

            return RedirectToAction("MyLeaveRequests");
        }

        if (selectedLeaveType != null && requestedDays > selectedLeaveType.DefaultDays)
        {
            ViewBag.AlertClass = "alert-danger";
            ViewBag.AlertMessage = "Requested days exceed the allowed limit for this leave type.";
        }
        else
        {
            ViewBag.AlertClass = "hidden"; 
            ViewBag.AlertMessage = "";
        }

        // Handle the case where the requested days exceed the allowed limit
        ModelState.AddModelError("EndDate", "Requested days exceed the allowed limit for this leave type.");
        Console.WriteLine("Den gubben gick inte");
        var leaveTypes = _context.LeaveTypes.ToList();
        ViewBag.LeaveTypes = new SelectList(leaveTypes, "Id", "Name");
        return View(leaveRequest);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id)
    {
        var leaveRequest = _context.LeaveRequest.FirstOrDefault(lr => lr.Id == id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        // Load status options (accepted and declined) as SelectList
        var statusOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "Pending", Text = "Pending" },
        new SelectListItem { Value = "Accepted", Text = "Accepted" },
        new SelectListItem { Value = "Declined", Text = "Declined" }
    };
        ViewBag.StatusOptions = new SelectList(statusOptions, "Value", "Text");

        return View(leaveRequest);
    }



    //Den här fungerar dåligt, någon konflikt LeaveRequest tabellen och LeaveType, Exception blir Castat
    //Använd dig av AccepRequest och DenyRequest för att hantera en användares LeaveRequest 
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Edit(LeaveRequest leaveRequest)
    {
        _context.LeaveRequest.Update(leaveRequest);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }


    [Authorize]
    public IActionResult Delete(int id)
    {
        var leaveRequest = _context.LeaveRequest.FirstOrDefault(lr => lr.Id == id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        var user = _userManager.GetUserAsync(User).Result;

        // Check if the user is an admin
        if (User.IsInRole("Admin"))
        {
            // Admins can delete any LeaveRequest
            _context.LeaveRequest.Remove(leaveRequest);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Check if the leave request belongs to the current user
        if (leaveRequest.EmployeeId != user.Id)
        {
            return Unauthorized();
        }

        // Non-admin users can only delete their own LeaveRequests
        _context.LeaveRequest.Remove(leaveRequest);
        _context.SaveChanges();
        return RedirectToAction("MyLeaveRequests");
    }

    [Authorize(Roles = "Employee")]
    public IActionResult MyLeaveRequests()
    {
        // Get the currently logged-in user
        var user = _userManager.GetUserAsync(User).Result;

        // Fetch LeaveRequests for the logged-in user
        var leaveRequests = _context.LeaveRequest
            .Include(lr => lr.LeaveType)
            .Where(lr => lr.EmployeeId == user.Id)
            .ToList();

        return View(leaveRequests);
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AcceptRequest(int id)
    {
        var leaveRequest = _context.LeaveRequest.FirstOrDefault(lr => lr.Id == id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        
        leaveRequest.Status = "Accepted";
       

        _context.LeaveRequest.Update(leaveRequest);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    public IActionResult DenyRequest(int id)
    {
        var leaveRequest = _context.LeaveRequest.FirstOrDefault(lr => lr.Id == id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        
        leaveRequest.Status = "Declined";
        

        _context.LeaveRequest.Update(leaveRequest);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}