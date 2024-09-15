using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        bool isFirstStartup = false;

        // Application Data
        public ApplicationData applicationData { get; set; }

        public LoginWindow()
        {
            InitializeComponent();
            applicationData = ApplicationData.LoadApplicationData(); // Load application data from the XML file
            InitializeAdminAccount();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = usernameTextBox.Text.Trim().ToLower();
            string password = passwordBox.Password.Trim();

            // Validate inputs
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                errorMessageTextBlock.Text = "Username and Password cannot be empty.";
                return;
            }

            if(isFirstStartup)
            {
                // Verify password
                Util.PasswordVerificationResponse response = Util.VerifyPassword(password);
                if (!response.IsVerified)
                {
                    errorMessageTextBlock.Text = response.ResponseMessage;
                    passwordBox.Clear();
                    return;
                }
  
                // Create new administrator account and save
                User newAdmin = new User(username, password, (int)Util.UserRole.ADMINISTRATOR);
                applicationData.DefaultAdminUsername = username;
                applicationData.Users_.Add(newAdmin);
                ApplicationData.SaveApplicationData(applicationData);

                // Notify user
                errorMessageTextBlock.Text = "";
                usernameTextBox.Clear();
                passwordBox.Clear();
                MessageBox.Show("New administrator account created! Please log in");

                // Switch to log in mode
                isFirstStartup = false;
            }
            else
            {
                // Check user credentials
                var user = applicationData.Users_.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    errorMessageTextBlock.Text = "User does not exist.";
                    usernameTextBox.Clear();
                    passwordBox.Clear();
                    return;
                }

                if (user.Password != password)
                {
                    errorMessageTextBlock.Text = "Incorrect password.";
                    passwordBox.Clear();
                    return;
                }

                MessageBox.Show($"Welcome, {user.Username}! You are logged in as {Util.GetUserRoleString(user.Role)}.");
                // Close login window and open main window if user is not a research student
                if (user.Role == (int)Util.UserRole.ADMINISTRATOR || user.Role == (int)Util.UserRole.STAFF_MEMBER )
                {
                    var mainWindow = new MainWindow(applicationData, user.Username, user.Role);
                    mainWindow.Show();
                }
                if (user.Role == (int)Util.UserRole.RESEARCH_STUDENT)
                {
                    // open window for research students
                    MessageBox.Show("Research Student Window WIP");
                }
                this.Close();
            }
        }

        private void InitializeAdminAccount()
        {
            string admin = applicationData.DefaultAdminUsername;
            if(string.IsNullOrEmpty(admin))
            {
                MessageBox.Show($"Zikit is running for the first time, please input a username and a password that will be used for the main administrator's account.");
                errorMessageTextBlock.Text = "CREATING NEW ADMINISTRATOR ACCOUNT";
                isFirstStartup = true;
            }
            else
            {
                isFirstStartup = false;
            }
        }
    }
}
