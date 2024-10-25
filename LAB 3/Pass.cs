using System;
using System.Collections.ObjectModel; 
using System.Collections.Generic;

namespace BookingSystem.Gym
{
    public class Pass
    {
        private const int MaxSpots = 3; // Maximum number of spots available for each session set to 3 
        public string Name { get; set; } // Customer name
        public DateTime Time { get; set; } // Time of session
        public string GymPass { get; set; } // Type of session 
        public int NumSpotsTotal { get; set; } = MaxSpots;
        public string AvailabilityStatus { get; set; } // Availabily 
        public string Category { get; set; } // Type of workout
        public int BookedSpots => BookedCustomers.Count; // Number of booked spots      
        public List<User> BookedCustomers { get; private set; } = new List<User>();  // List of booked customers
        public static List<string> GymSession { get; } = new List<string>();  // Static list for gym sessions
        public string AvailableSpots => $"{BookedSpots}/{MaxSpots}";  // Property to show booked spots


        // Static constructor to initialize available sessions
        static Pass()
        {
            string[] sessions = { "Yoga", "Spinning", "LesMills", "Pilates", "Zumba" }; // Name/category of sessions
            GymSession.AddRange(sessions); 
        }


        // Constructor to create a new pass
        public Pass(string name, string pass, DateTime time, int numSpotsTotal) //time, category, number of spots, fully booked 
        {
            Name = name;
            Time = time;
            GymPass = pass;
            NumSpotsTotal = numSpotsTotal;
        }


        // Method to check if a spot is available
        public bool HasAvailableSpot()
        {
            return BookedCustomers.Count < MaxSpots; 
        }


        // Method to book a session for a customer
        public string Book(User userCustomer)
        {
            if (HasAvailableSpot()) // Check if there is an available spot
            {
                BookedCustomers.Add(userCustomer); // Add the customer to the booked list
                return $"Booking successful for {userCustomer.FullName} for {GymPass} at {Time:HH:mm}.";
            }
            else
            {
                return $"This session is fully booked."; // Inform that the session is fully booked
            }
        }


        // Property to determine if the session is booked
        public bool IsBooked
        {
            get
            {
                return BookedCustomers.Count >= MaxSpots;
            }
        }


        // Check if the booked customer is the current user
        public bool IsItBookedByUser(User user)
        {
            foreach (var bookedUser in BookedCustomers)
            { 
                if (bookedUser.UserId == user.UserId)
                { 
                    return true;
                }
            }

            return false;
        }


        public override string ToString()
        {
            return $"{Name} - {GymPass} at {Time:HH:mm}";
        }
    }
}
