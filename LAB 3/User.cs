using System;

namespace BookingSystem.Gym
{
    public class User
    {
        // Properties
        private string FirstName { get; set; } // Firstname 
        private string LastName { get; set; } // Secondname
        public int UserId { get; private set; } //UserID
        public string Password { get; private set; } // Password
        public bool IsMember { get; private set; } // Member status
        public string FullName => $"{FirstName} {LastName}"; // Full name first + last


        // Constructor
        public User(string firstName, string lastName, int userId, string password, bool isMember)
        {
            FirstName = firstName;
            LastName = lastName;
            UserId = userId;
            Password = password;
            IsMember = isMember;
        }

        public void SetMembershipStatus(bool isMember)
        {
            IsMember = isMember; 
        }


        // Display user's name
        public string GetUserName()
        {
            return FullName; // Full name
        }
    }
}

