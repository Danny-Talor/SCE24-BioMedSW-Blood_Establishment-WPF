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
    public partial class ChangeRoleWindow : Window
    {
        private User userToChange;

        public ChangeRoleWindow(User user)
        {
            InitializeComponent();
            userToChange = user;
            CurrentRoleTextBlock.Text = Util.GetUserRole(user.Role);
            RoleComboBox.SelectedItem = Util.GetUserRole(user.Role);
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            var newRole = RoleComboBox.SelectedItem.ToString();
            userToChange.Role = Util.GetUserRole(newRole);
            DialogResult = true;
            Close();
        }
    }
}
