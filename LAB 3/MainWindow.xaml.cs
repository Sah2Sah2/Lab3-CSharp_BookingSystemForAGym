using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace BookingSystem.Gym
{
    public partial class MainWindow 
    {
        private BookingSystem bookingSystem;

        private User currentUser; // Property to hold the current user
        public DateTime Today { get; } = DateTime.Today;
        public DateTime Tomorrow { get; } = DateTime.Today.AddDays(1);

        public MainWindow()
        {
            InitializeComponent();
            bookingSystem = new BookingSystem(); // Initialize booking system
            bookingSystem.InitializeSessions();
            SessionsListView.ItemsSource = bookingSystem.GetAvailableSessions();
            SessionsListView.Visibility = Visibility.Collapsed; // Ensure the session list is hidden initially
            DataContext = this; // Set DataContext to access these properties in XAML
            SetDatePickerConstraints();
        }

        //Cursor
        private void CursorTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox source = e.Source as ComboBox;

            if (source != null)
            {
                ComboBoxItem selectedCursor = source.SelectedItem as ComboBoxItem;

                // Changing the cursor of the DisplayArea
                if (selectedCursor != null)
                {
                    switch (selectedCursor.Content.ToString())
                    {
                        case "Pen":
                            DisplayArea.Cursor = Cursors.Pen;
                            break;
                    }
                }
            }
        }


        // Allow only today, tomorrow, and the day after to be shown in the calender during the search 
        private void SetDatePickerConstraints()
        {
            DateTime today = DateTime.Today;
            SearchDatePicker.DisplayDateStart = today; // Start from today
            SearchDatePicker.DisplayDateEnd = today.AddDays(2); // End after the day after tomorrow

            SearchDatePicker.SelectedDate = today; 
        }


        private string CurrentUserId; // Track the current user


        // Login button
        private void LoginButton_Click(Object sender, RoutedEventArgs e)
        {
        CurrentUserId = UserIdTextBox.Text; // Set the current user ID /////////
        LoginPanel.Visibility = Visibility.Collapsed;
        LoginPanel2.Visibility = Visibility.Visible;
        }


        // Event handler for exiting the current user session 
        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUserId = null; // Reset current user ID 

            SecondMenuPanel.Visibility = Visibility.Collapsed;
            LoginPanel2.Visibility = Visibility.Collapsed;
            LoginPanel.Visibility = Visibility.Visible;

            // Clear the User Id and Password fields
            UserIdTextBox.Clear();
            PasswordBox.Clear();
            MessageBox.Show("You have logged out successfully.");

            SessionsListView.ItemsSource = null;
            SessionsListView.Visibility = Visibility.Collapsed;
        }

        // Handle the login button click
        private void ConfirmLoginButton_Click(object sender, RoutedEventArgs e)
        {
            string userIdText = UserIdTextBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            //// Debugging: Show username and password values
            //MessageBox.Show($"UserId: {userIdText}\nPassword: {password}");

            if (int.TryParse(userIdText, out int userId)) // Parse the userId input to an integer
            {
                var user = bookingSystem.customersList
                    .FirstOrDefault(customer => customer.UserId == userId && customer.Password == password);  // Check if the user credentials are valid

                if (user != null) // Matching user 
                {
                    currentUser = user; 
                    currentUser.SetMembershipStatus(true);
                    
                    LoginPanel.Visibility = Visibility.Collapsed;
                    LoginPanel2.Visibility = Visibility.Collapsed;
                    SecondMenuPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    // Show message box
                    MessageBoxResult result = MessageBox.Show(
                        "Invalid login credentials. Would you like to retry (yes) or exit (no)?",
                        "Login Failed",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.Yes);

                    if (result == MessageBoxResult.Yes) // Retry
                    {
                        // Keep the login panel visible for retry
                        LoginPanel2.Visibility = Visibility.Visible;
                        LoginPanel.Visibility = Visibility.Collapsed; 
                    }
                    else if (result == MessageBoxResult.No) // Exit
                    {
                        Application.Current.Shutdown();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid User Id (numeric).");
            }
        }


        // Method to clear out any existing state for the new user
        private void RefreshUserSessions(User currentUser)
        {
            var bookedSessions = bookingSystem.GetBookedSessionsByUser(currentUser); // Get the booked sessions for the logged-in user

            // Update the UI with the booked sessions
            SessionsListView.ItemsSource = bookedSessions;
            SessionsListView.Items.Refresh();
        }


        //Method to check if customer exist by their ID
        public User GetUserById(int userId)
        {
            for (int i = 0; i < bookingSystem.customersList.Count; i++)
            {
                User customer = bookingSystem.customersList[i];
                if (customer.UserId == userId)
                {
                    return customer;
                }
            }
            return null;
        }


        // Method to register a user 
        private void LogInUser()
        {
            string userIdInput = PromptForInput("Enter your ID:", "User ID", "XXXX");  // Prompt for customer ID

            if (!int.TryParse(userIdInput, out int userId))
            {
                MessageBox.Show("Please enter a valid ID.");
                return;
            }

            currentUser = GetUserById(userId);
            MessageBox.Show($"Welcome, {currentUser.GetUserName()}!");
        }


        // Method to prompt for user input
        private string PromptForInput(string message, string title, string defaultValue)
        {
            return Microsoft.VisualBasic.Interaction.InputBox(message, title, defaultValue);
        }


        // Method to get available sessions for the rest of the current week
        public ObservableCollection<Pass> GetRemainingSessionsForWeek()
        {
            ObservableCollection<Pass> filteredSessions = new ObservableCollection<Pass>();
            DateTime today = DateTime.Today;
            DateTime currentTime = DateTime.Now;

            // Loop through the available sessions and filter
            foreach (var session in bookingSystem.AvailableSessions)
            {
                DayOfWeek sessionDay = session.Time.DayOfWeek; // Get the day of the week for the session
      
                if (session.Time.Date == today) // Check if the session is today or later in the current week
                {
                    if (session.Time > currentTime)  // If the session is today, time is later than the current time
                    {
                        filteredSessions.Add(session);
                    }
                }
                else if (session.Time.Date > today && sessionDay <= DayOfWeek.Sunday)
                {
                    filteredSessions.Add(session);
                }
            }

            return filteredSessions;
        }


        // Method to update the sessions
        private void ViewSessions_Click(object sender, RoutedEventArgs e)
        {
            var upcomingSessions = bookingSystem.GetRemainingSessionsForWeek(); // Get the upcoming sessions for the week
           
            bookingSystem.AvailableSessions.Clear();  // Clear the existing list and bind the filtered sessions
            foreach (var session in upcomingSessions)
            {
                bookingSystem.AvailableSessions.Add(session);
            }

            SessionsListView.ItemsSource = null; //clear bidning
            SessionsListView.ItemsSource = bookingSystem.GetAvailableSessionsNotBookedByUser(currentUser);

            SessionsListView.Visibility = Visibility.Visible;
        }


        // Event handler for booking a session
        private void BookSession_Click(object sender, RoutedEventArgs e)
        {
            // Show available sessions and allow booking
            ViewSessions_Click(sender, e); // Display sessions
            MessageBox.Show("Please select a session to book.");
        }
        

        //Event handler to view booked sessions
        private void ViewBookedSessions_Click(object sender, RoutedEventArgs e)
        {
            var bookedSessions = bookingSystem.GetBookedSessionsByUser(currentUser);  // Get the booked sessions for the user

            SessionsListView.ItemsSource = bookedSessions;  

            SessionsListView.Visibility = bookedSessions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        
        // Event handler to book the selected session
        private void BookSelectedSession_Click(object sender, RoutedEventArgs e)
        {
            Pass selectedSession = (Pass)SessionsListView.SelectedItem; // Ensure that a session is selected

            if (selectedSession == null)
            {
                MessageBox.Show("Please select a session to book.");
                return; 
            }

            string result = bookingSystem.BookSession(selectedSession, currentUser);  // Book the selected session

            MessageBox.Show(result); // Show the result of the booking 

            ViewSessions_Click(sender, e); // Refresh the sessions view after booking
        }


        //Event handler to cancel a session
        private void CancelSession_Click(object sender, RoutedEventArgs e)
        {
            // Show booked sessions and allow cancelling
            ViewBookedSessions_Click(sender, e); 
            MessageBox.Show("Please select a session from the list to cancel.");
        }


        // Event handler for exiting the application
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        // Method to get the current user
        private User GetCurrentUser()
        {
            return currentUser; 
        }

   
        // Event handler for selection changes in the ListView
        private void SessionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SessionsListView.SelectedItem is Pass selectedSession) // Check if a session is selected
            {   
                if (selectedSession.IsItBookedByUser(currentUser))
                {
                    string bookingMessage = bookingSystem.CancelSession(selectedSession, currentUser);
                    MessageBox.Show(bookingMessage);

                    // Clear the ListView after cancelling
                    SessionsListView.ItemsSource = null;
                    SessionsListView.ItemsSource = bookingSystem.GetBookedSessionsByUser(currentUser);
                }

                // Check if there is an available spot
                else if (selectedSession.HasAvailableSpot())
                {
                    User currentUser = GetCurrentUser();  // Get the current user

                    if (currentUser != null)
                    {
                        string bookingMessage = selectedSession.Book(currentUser);  // Book the session for the current user

                        MessageBox.Show(bookingMessage, "Booking Status", MessageBoxButton.OK, MessageBoxImage.Information); // Display booking message

                        var bookingInfo = (UserId: currentUser.UserId, UserName: currentUser.FullName, GymPass: selectedSession.GymPass, Time: selectedSession.Time);  // Create a booking tuple with relevant information

                        SessionsListView.ItemsSource = null; // Clear the binding
                        SessionsListView.ItemsSource = bookingSystem.GetAvailableSessionsNotBookedByUser(currentUser); // Re-bind
                    }
                    else
                    {
                        MessageBox.Show("No user is currently logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("This session is fully booked.", "Booking Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }


        // Method to search for sessions based on date, time, and/or category
        public List<Pass> SearchSessions(string timeInput, string categoryInput, DateTime selectedDate)
        {
            DateTime searchTime;
            bool isTimeParsed = DateTime.TryParse(timeInput, out searchTime);

            var allSessions = bookingSystem.GetAvailableSessions(); // Retrieve all sessions, even if already booked by the user

            // Filter sessions based on date, time, and category and match them
            var results = allSessions 
                .Where(session =>
                    session.Time.Date == selectedDate.Date &&
                    (string.IsNullOrEmpty(categoryInput) || session.GymPass.Equals(categoryInput, StringComparison.OrdinalIgnoreCase)) &&
                    (!isTimeParsed || session.Time.TimeOfDay == searchTime.TimeOfDay))
                .ToList();

            return results;
        }


        // Event handler to search sessions based on name and time
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTime = SearchTimeComboBox.Text; // Input time
            string searchPass = SearchPassTextBox.Text;  // Input name
            DateTime selectedDate = SearchDatePicker.SelectedDate ?? DateTime.Today; // Input date

            try
            {
                var searchResults = SearchSessions(searchTime, searchPass, selectedDate); // Search

                if (searchResults.Count > 0)
                {
                    // Check if any of the sessions are already booked by the current user
                    bool isAlreadyBooked = searchResults.Any(session => session.IsItBookedByUser(currentUser));

                    string message = isAlreadyBooked
                        ? "Session found. You already booked it."
                        : "Session found.";

                    MessageBoxResult result = MessageBox.Show(message,
                                                              "Confirm Action",
                                                              MessageBoxButton.OK);

                    SessionsListView.ItemsSource = searchResults; 

                    SessionsListView.Visibility = Visibility.Visible;  
                }
                else
                {
                    MessageBox.Show("No sessions found matching the search criteria.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Handle any errors
            }
        }

        
        // Search through the different passes on the calender
        private void SearchDatePicker_Loaded(object sender, RoutedEventArgs e)
        {
            DateTime today = DateTime.Today; // Starting from today

            SearchDatePicker.DisplayDateStart = today;
            SearchDatePicker.DisplayDateEnd = today.AddDays(2); // Dispaly only two days more 
            SearchDatePicker.BlackoutDates.Clear();

            for (int i = 3; i < 30; i++) // Show only passes for today, tomorrow, and after tomorrow
            {
                SearchDatePicker.BlackoutDates.Add(new CalendarDateRange(today.AddDays(i)));
            }
        }
    }
}
