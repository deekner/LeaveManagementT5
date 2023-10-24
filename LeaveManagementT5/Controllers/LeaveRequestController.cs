using LeaveManagementT5.Data;
using LeaveManagementT5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaveManagementT5.Services;

[Authorize]
public class LeaveRequestController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;

    public LeaveRequestController(UserManager<IdentityUser> userManager, AppDbContext context, IEmailSender emailSender)
    {
        _userManager = userManager;
        _context = context;
        _emailSender = emailSender;
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

        if (TempData.ContainsKey("InsufficientLeaveDaysMessage"))
        {
            ViewBag.InsufficientLeaveDaysMessage = TempData["InsufficientLeaveDaysMessage"].ToString();
        }

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
        
        leaveRequest.Status = "Pending";

        var user = _userManager.GetUserAsync(User).Result;
        leaveRequest.EmployeeId = user.Id;

        
        int requestedDays = (leaveRequest.EndDate - leaveRequest.StartDate).Days;

        var selectedLeaveType = _context.LeaveTypes.FirstOrDefault(lt => lt.Id == leaveRequest.LeaveTypeId);

        if (selectedLeaveType != null && requestedDays <= selectedLeaveType.DefaultDays)
        {
            
            _context.LeaveRequest.Add(leaveRequest);
            _context.SaveChanges();
            return RedirectToAction("MyLeaveRequests");
        }
        else
        {
           
            ViewBag.AlertClass = "alert-danger";
            ViewBag.AlertMessage = "Requested days exceed the allowed limit for this leave type.";
            
            ModelState.AddModelError("EndDate", "Requested days exceed the allowed limit for this leave type.");
        }

        
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
    public async Task<IActionResult> AcceptRequest(int id)
    {
        var leaveRequest = _context.LeaveRequest.FirstOrDefault(lr => lr.Id == id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        if(leaveRequest.Status == "Pending")
        {
            int daysToDeduct = (leaveRequest.EndDate - leaveRequest.StartDate).Days;

            var allocatedLeave = _context.LeaveAllocation.FirstOrDefault(la => la.EmployeeId == leaveRequest.EmployeeId && la.LeaveTypeId == leaveRequest.LeaveTypeId);

            if(allocatedLeave != null)
            {
                


                if (allocatedLeave.NumberOfDays >= daysToDeduct)
                {
                    allocatedLeave.NumberOfDays -= daysToDeduct;

                    leaveRequest.Status = "Accepted";

                    _context.LeaveAllocation.Update(allocatedLeave);

                    _context.LeaveRequest.Update(leaveRequest);

                    _context.SaveChanges();
                }
                else
                {
                    TempData["InsufficientLeaveDaysMessage"] = "The employee does not have enough days to take out this request. Consider editing the employees Allocation days or just straight up deny the request ";
                    return RedirectToAction("Index");

                }
            }
        }







        //leaveRequest.Status = "Accepted";

        ////------------------------------- 

        //var emailaddresses = _context.Users.Select(x => x.Email); //Finds the Email of select user depending on LeaveRequest
        //foreach (var emailaddress in emailaddresses) //Loops through all emails and sends message depending on user's email
        //{
        //    var receiver = emailaddress;
        //    var subject = "Leave Request";
        //    var message = "You've got a message from the Leave management system";

        //    await _emailSender.SendEmailAsync(receiver, subject, message);
        //}

        //_context.LeaveRequest.Update(leaveRequest);
        //_context.SaveChanges();

        return RedirectToAction("Index");
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DenyRequest(int id)
    {
        var leaveRequest = _context.LeaveRequest.FirstOrDefault(lr => lr.Id == id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        
        leaveRequest.Status = "Declined";



        //var emailaddresses = _context.Users.Select(x => x.Email);
        //foreach (var emailaddress in emailaddresses)
        //{
        //    var receiver = emailaddress;
        //    var subject = "Leave Request";
        //    var message = "You've got a message from the Leave management system";

        //    await _emailSender.SendEmailAsync(receiver, subject, message);
        //}


        _context.LeaveRequest.Update(leaveRequest);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }
}