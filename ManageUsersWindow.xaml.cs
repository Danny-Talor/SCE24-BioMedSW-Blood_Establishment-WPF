using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class ManageUsersWindow : Window
    {
        private ObservableCollection<User> users;
        public ApplicationData ApplicationData { get; set; }

        public ManageUsersWindow(ApplicationData ApplicationData)
        {
            InitializeComponent();

            this.ApplicationData = ApplicationData;
            users = new ObservableCollection<User>(
                ApplicationData.Users_.Where(u => u.Role != (int)Util.UserRole.ADMINISTRATOR)
            );
            UsersDataGrid.ItemsSource = users;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            // Open the Add User Window
            var addUserWindow = new AddUserWindow(users);
            if (addUserWindow.ShowDialog() == true)
            {
                users.Add(addUserWindow.NewUser);
                ApplicationData.Users_.Add(addUserWindow.NewUser);
                ApplicationData.SaveApplicationData(ApplicationData);
            }
        }
        private void ChangeRole_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = ((FrameworkElement)sender).DataContext as User;
            var changeRoleWindow = new ChangeRoleWindow(selectedUser);

            if (changeRoleWindow.ShowDialog() == true)
            {
                // Refresh the ObservableCollection to reflect the updated user role
                UsersDataGrid.Items.Refresh();
                ApplicationData.SaveApplicationData(ApplicationData);
            }
        }

        private void ResetPassword_Click(object sender, RoutedEventArgs e)
        {
            // Find selected user and reset password
            var selectedUser = ((FrameworkElement)sender).DataContext as User;
            var user = ApplicationData.Users_.FirstOrDefault(u => u.Username == selectedUser.Username);
            user.Password = Util.DefaultPassword;
            user.IsPasswordChangeRequired = true;
            ApplicationData.SaveApplicationData(ApplicationData);
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var user = ((FrameworkElement)sender).DataContext as User;
            users.Remove(user);
            ApplicationData.Users_.Remove(user);
            ApplicationData.SaveApplicationData(ApplicationData);
        }
    }
}
