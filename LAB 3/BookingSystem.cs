using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BookingSystem.Gym
{
    public class BookingSystem
    {
        public ObservableCollection<Pass> AvailableSessions { get; private set; } = new ObservableCollection<Pass>();
        public List<User> customersList; // List to store all registered users
        private List<(int UserId, string UserName, string GymPass, DateTime Time)> bookings; // List to store booking information 
        private bool cursorScopeElementOnly = false; // Change the mouse pointer when over a specific element 

        public BookingSystem()
        {
            InitializeSessions(); // Initialize available sessions
            InitializeCustomers(); // Initialize predefined customers
        }


        // Method to initialize gym sessions 
        public void InitializeSessions()
        {
            AvailableSessions.Clear(); // Clear previous sessions
            int numberOfDays = 3; // Limit to 3 days: today, tomorrow, the day after tomorrow 
            DateTime startDate = DateTime.Today; // Start from today's date

            for (int i = 0; i < numberOfDays; i++)  // Loop through each day to add sessions at the same time
            {
                DateTime currentDate = startDate.AddDays(i);

                // Assign specific times for each session on the current day
                // Morning
                DateTime yogaTimeMorning = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 11, 0, 0); // 11:00 
                DateTime spinningTimeMorning = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 9, 0, 0); // 09:00 
                DateTime lesMillsTimeMorning = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 10, 0, 0); // 10:00
                DateTime pilatesTimeMorning = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 9, 0, 0); // 09:00
                DateTime zumbaTimeMorning = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 11, 0, 0); // 11:00

                // Afternoon/Evening
                DateTime yogaTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 21, 0, 0); // 21:00 
                DateTime spinningTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 16, 0, 0); // 16:00 
                DateTime lesMillsTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 17, 0, 0); // 17:00
                DateTime pilatesTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 20, 0, 0); // 20:00
                DateTime zumbaTime = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, 21, 0, 0); // 21:00

                AvailableSessions.Add(new Pass("", "Yoga", yogaTimeMorning, 3));
                AvailableSessions.Add(new Pass("", "Spinning", spinningTimeMorning, 3));
                AvailableSessions.Add(new Pass("", "LesMills", lesMillsTimeMorning, 3));
                AvailableSessions.Add(new Pass("", "Pilates", pilatesTimeMorning, 3));
                AvailableSessions.Add(new Pass("", "Zumba", zumbaTimeMorning, 3));
                AvailableSessions.Add(new Pass("", "Yoga", yogaTime, 3));
                AvailableSessions.Add(new Pass("", "Spinning", spinningTime, 3));
                AvailableSessions.Add(new Pass("", "LesMills", lesMillsTime, 3));
                AvailableSessions.Add(new Pass("", "Pilates", pilatesTime, 3));
                AvailableSessions.Add(new Pass("", "Zumba", zumbaTime, 3));
            }
        }


        // Method to initialize predefined customers
        private void InitializeCustomers()
        {
            customersList = new List<User> // List with predefined users
            {
                new User("Zoe", "Larsson", 1010, "dog", true),
                new User("Jane", "Smith", 2020, "cat", true),
                new User("Emily", "Johnson", 3030, "bird", true),
                new User("Hannah", "Borg", 4040, "lion", true),
            };
        }


        // Method to register a new customer
        public void RegisterCustomer(string firstName, string lastName, int userId, string password, bool isMember)
        {
            User newUser = new User(firstName, lastName, userId, password, isMember);
            customersList.Add(newUser); // Add new user to the customer list
        }


        // Method to get the list of all registered customers
        public List<User> GetCustomers()
        {
            return customersList;
        }


        // Method to find a customer by their ID
        public User GetCustomerById(int userId)
        {
            foreach (User customer in customersList)
            {
                if (customer.UserId == userId)
                {
                    return customer; 
                }
            }
            return null; 
        }


        // Method to get passes of today, tomorrw, and the day after tomorrow
        public ObservableCollection<Pass> GetRemainingSessionsForWeek()
        {
            ObservableCollection<Pass> filteredSessions = new ObservableCollection<Pass>();
            DateTime today = DateTime.Today;
            DateTime currentTime = DateTime.Now;

            foreach (var session in AvailableSessions)
            {
                DateTime sessionDate = session.Time.Date;

                if (sessionDate == today) // Check if the session is today
                {
                    if (session.Time > currentTime)  // If the session is today, time is later than the current time
                    {
                        filteredSessions.Add(session);
                    }
                }
                else if (sessionDate == today.AddDays(1) || sessionDate == today.AddDays(2)) // Check if the session is tomorrow or the day after tomorrow
                {
                    filteredSessions.Add(session);
                }
            }

            return filteredSessions;
        }


        // Method to get sessions available for booking by any user
        public ObservableCollection<Pass> GetAvailableForBookingSessions()
        {
            ObservableCollection<Pass> availableForBookingSessions = new ObservableCollection<Pass>();

            foreach (var session in AvailableSessions)
            {
                if (session.BookedCustomers.Count < session.NumSpotsTotal) // If the session is not fully booked, add it to the available list
                {
                    session.AvailabilityStatus = "Available for booking";
                    availableForBookingSessions.Add(session);
                }
            }

            return availableForBookingSessions;
        }


        // Method to book a session 
        public string BookSession(Pass session, User userCustomer)
        {

            // Check if there are available spots for the session
            if (session.BookedCustomers.Count >= 3) 
            {
                return "Sorry, this session is fully booked."; // Session is fully booked
            }

            session.BookedCustomers.Add(userCustomer); // Add the customer to the session booking

            var bookingInfo = (UserId: userCustomer.UserId, UserName: userCustomer.GetUserName(), GymPass: session.GymPass, Time: session.Time);

            return $"Booking successful for {userCustomer.GetUserName()} for {session.GymPass} at {session.Time:HH:mm}."; // Confirmation message
        }


        // Method to get booked sessions 
        public ObservableCollection<Pass> GetBookedSessionsByUser(User user)
        {
            ObservableCollection<Pass> bookedSessions = new ObservableCollection<Pass>();

            foreach (var session in AvailableSessions)
            {
                if (session.BookedCustomers.Contains(user))
                {
                    session.AvailabilityStatus = "Booked"; 
                    bookedSessions.Add(session);
                }

                else
                {
                    session.AvailabilityStatus = "Available";
                }
            }

            return bookedSessions;
        }


        // Method to cancel a session
        public string CancelSession(Pass session, User userCustomer)
        {
            if (session.BookedCustomers.Contains(userCustomer)) // Check if the session is booked by the customer
            {
                session.BookedCustomers.Remove(userCustomer); // Remove the customer from the booking
                session.AvailabilityStatus = "Available"; // Status after cancellation
                return $"Booking for {userCustomer.GetUserName()} for {session.GymPass} at {session.Time:HH:mm} has been canceled.";
            }
            else
            {
                return "There is no booking with your name";
            }
        }


        // Method to get all available sessions
        public ObservableCollection<Pass> GetAvailableSessions()
        {
            return AvailableSessions; // List of available sessions
        }


        // Method to get all available sessions NOT booked by the current user
        public ObservableCollection<Pass> GetAvailableSessionsNotBookedByUser(User user)
        {
            ObservableCollection<Pass> AvailableSessionsForUser = new ObservableCollection<Pass>();

            foreach (var session in AvailableSessions)
            {
                if (!session.BookedCustomers.Contains(user))
                {
                    AvailableSessionsForUser.Add(session);
                    session.AvailabilityStatus = "Available";
                }
            }

            return AvailableSessionsForUser;
        }
    }
}
