using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
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
            bool isValid = true;

            // Validate Full Name
            string fullName = FullNameTextBox.Text;
            if (string.IsNullOrEmpty(fullName) || !Regex.IsMatch(fullName, @"^[A-Z][a-zA-Z]{1,20}(?: [A-Z][a-zA-Z]{1,20}){0,2}$"))
            {
                FullNameError.Text = "Full name invalid";
                FullNameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                FullNameError.Visibility = Visibility.Collapsed;
            }

            // Validate Identification Number
            string idNumber = IdentificationNumberTextBox.Text;
            if (string.IsNullOrEmpty(idNumber) || !Util.IsValidIsraeliIDNumber(idNumber))
            {
                IdNumberError.Text = "Invalid Israeli ID number.";
                IdNumberError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                IdNumberError.Visibility = Visibility.Collapsed;
            }

            // Validate Blood Type
            if (BloodTypeComboBox.SelectedItem == null)
            {
                BloodTypeError.Text = "Please select a blood type.";
                BloodTypeError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                BloodTypeError.Visibility = Visibility.Collapsed;
            }

            // Validate Donation Date
            DateTime? selectedDate = DonationDatePicker.SelectedDate;
            if (selectedDate == null || selectedDate > DateTime.Today)
            {
                DateError.Text = "Please select a valid date.";
                DateError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                DateError.Visibility = Visibility.Collapsed;
            }

            return isValid;
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

        private void Image_ToolTipOpening(object sender, ToolTipEventArgs e)
        {

        }
    }
}