using DocumentFormat.OpenXml.Spreadsheet;
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
    public partial class AddUserWindow : Window
    {
        public User NewUser { get; private set; }
        ObservableCollection<User> users;

        public AddUserWindow(ObservableCollection<User> users)
        {
            InitializeComponent();
            this.users = users;
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Username cannot be empty.");
                return;
            }

            var user = users.FirstOrDefault(u => u.Username == username);
            if(user != null)
            {
                MessageBox.Show("Username already exists");
                return;
            }


            NewUser = new User
            {
                Username = UsernameTextBox.Text.Trim().ToLower(),
                Role = Util.GetUserRole(RoleComboBox.SelectedItem.ToString()),
                Password = Util.DefaultPassword
            };

            DialogResult = true;
            this.Close();
        }
    }
}
