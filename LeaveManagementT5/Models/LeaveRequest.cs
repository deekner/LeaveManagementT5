namespace LeaveManagementT5.Models
{
    public class LeaveRequest
    {

        public int Id { get; set; }

        public string EmployeeId { get; set; }

        public ApplicationUser Employee { get; set; }

        public int LeaveTypeId { get; set; }

        public LeaveType LeaveType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Status { get; set; }
    }
}
