namespace LeaveManagementT5.Models
{
    public class LeaveAllocation
    {

        public int Id { get; set; }
        public string EmployeeId { get; set; } 
        public ApplicationUser Employee { get; set; } 
        public int LeaveTypeId { get; set; } 
        public LeaveType LeaveType { get; set; } 
        public int NumberOfDays { get; set; }
        public int Year { get; set; }
    }
}
