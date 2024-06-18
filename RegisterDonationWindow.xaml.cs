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
    /// <summary>
    /// Interaction logic for RegisterDonationWindow.xaml
    /// </summary>
    public partial class RegisterDonationWindow : Window
    {
        public event EventHandler<Donation> DonationSubmitted;
        private ObservableCollection<Donation> Donations { get; set; }

        public RegisterDonationWindow(ObservableCollection<Donation> donations)
        {
            InitializeComponent();
            Donations = donations;
            IdentificationNumberTextBox.TextChanged += IdentificationNumberTextBox_TextChanged;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                var donation = new Donation
                {
                    FullName = FullNameTextBox.Text,
                    IdentificationNumber = IdentificationNumberTextBox.Text,
                    BloodType = (BloodTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    DonationDates = { DonationDatePicker.SelectedDate ?? DateTime.Now }
                };

                DonationSubmitted?.Invoke(this, donation);
                Close();
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(FullNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(IdentificationNumberTextBox.Text) ||
                BloodTypeComboBox.SelectedItem == null ||
                DonationDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error");
                return false;
            }
            return true;
        }

        private void IdentificationNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string identificationNumber = IdentificationNumberTextBox.Text;

            if (!string.IsNullOrWhiteSpace(identificationNumber))
            {
                Donation existingDonation = Donations.FirstOrDefault(d => d.IdentificationNumber == identificationNumber);

                if (existingDonation != null)
                {
                    FullNameTextBox.Text = existingDonation.FullName;
                    BloodTypeComboBox.SelectedItem = BloodTypeComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == existingDonation.BloodType);

                    FullNameTextBox.IsEnabled = false;
                    BloodTypeComboBox.IsEnabled = false;
                }
                else
                {
                    FullNameTextBox.Text = string.Empty;
                    BloodTypeComboBox.SelectedItem = null;
                    DonationDatePicker.SelectedDate = null;

                    FullNameTextBox.IsEnabled = true;
                    BloodTypeComboBox.IsEnabled = true;
                    DonationDatePicker.IsEnabled = true;
                }
            }
        }

    }
}
