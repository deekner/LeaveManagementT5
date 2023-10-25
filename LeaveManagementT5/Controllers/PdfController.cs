using DinkToPdf.Contracts;
using DinkToPdf;
using LeaveManagementT5.Data;
using LeaveManagementT5.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagementT5.Controllers
{
    [Authorize]
    public class PdfController : Controller
    {
        private readonly IConverter _converter;
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PdfController(IConverter converter, AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _converter = converter;
            _context = context;
            _userManager = userManager; // Assign UserManager
        }

        public IActionResult GeneratePdf()
        {
            // Get the currently logged-in user
            var user = _userManager.GetUserAsync(User).Result; // Make sure you have _userManager injected into your controller.

            // Fetch LeaveRequests for the logged-in user
            var leaveRequests = _context.LeaveRequest
                .Include(lr => lr.Employee)
                .Include(lr => lr.LeaveType)
                .Where(lr => lr.EmployeeId == user.Id) // Filter by the user's ID
                .ToList();

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
            };

            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = GeneratePdfHtmlContent(leaveRequests),
                WebSettings = { DefaultEncoding = "utf-8" },
                HeaderSettings = { FontName = "Arial", FontSize = 9 },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Right = "Page [page] of [toPage]" },
            };

            var pdf = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };

            byte[] pdfData = _converter.Convert(pdf);
            return File(pdfData, "application/pdf");
        }

        private string GeneratePdfHtmlContent(List<LeaveRequest> leaveRequests)
        {
            // Start with an HTML document and add a table for better formatting
            string htmlContent = "<html><body><h1>Leave Requests</h1><table border='1' width='100%'>";

            // Add table headers including "Status," "Days," and other columns
            htmlContent += "<tr><th>Employee</th><th>Leave Type</th><th>Start Date</th><th>End Date</th><th>Days</th><th>Status</th></tr>";

            foreach (var request in leaveRequests)
            {
                // Calculate the number of days between StartDate and EndDate
                int numberOfDays = (request.EndDate - request.StartDate).Days;

                // Add a row for each leave request including "Days" and "Status"
                htmlContent += $"<tr><td>{request.Employee.UserName}</td><td>{request.LeaveType.Name}</td><td>{request.StartDate.ToShortDateString()}</td><td>{request.EndDate.ToShortDateString()}</td><td>{numberOfDays}</td><td>{request.Status}</td></tr>";
            }

            // Close the table and the HTML document
            htmlContent += "</table></body></html>";

            return htmlContent;
        }
    }

}
