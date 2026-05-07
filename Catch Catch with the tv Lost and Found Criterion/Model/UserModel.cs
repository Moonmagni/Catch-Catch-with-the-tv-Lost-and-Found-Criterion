namespace Catch_Catch_with_the_tv_Lost_and_Found_Criterion.Model
{
    public class UserModel
    {
        // Primary Key (from your ERD: Users / Employees)
        public int EmployeeID { get; set; }

        // Employee / Admin Name
        public string EmployeeName { get; set; } = string.Empty;

        // Optional: for login tracking or audit logs
        public DateTime DateLogged { get; set; } = DateTime.Now;

        // Optional (useful for MVVM login system)
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}