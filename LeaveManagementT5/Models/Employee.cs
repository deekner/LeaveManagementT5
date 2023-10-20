using Microsoft.AspNetCore.Identity;

namespace LeaveManagementT5.Models
{
    public class Employee 
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        
    }
}
