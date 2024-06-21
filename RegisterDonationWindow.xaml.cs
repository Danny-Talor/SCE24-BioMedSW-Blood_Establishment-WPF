// RegisterDonationWindow.xaml.cs

using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    /// <summary>
    /// Interaction logic for RegisterDonationWindow.xaml
    /// </summary>
    public partial class RegisterDonationWindow : Window
    {
        // Event for submitting a donation
        public event EventHandler<Donation> DonationSubmitted;

        // Collection of existing donations
        private ObservableCollection<Donation> Donations { get; set; }

        // Constructor
        public RegisterDonationWindow(ObservableCollection<Donation> donations)
        {
            InitializeComponent();
            Donations = donations;
            IdentificationNumberTextBox.TextChanged += IdentificationNumberTextBox_TextChanged;
        }

        // Event handler for Submit button click
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

                // Invoke donation submitted event
                DonationSubmitted?.Invoke(this, donation);
                Close();
            }
        }

        // Method to validate input fields
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

        // Event handler for Identification Number text box text changed
        private void IdentificationNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string identificationNumber = IdentificationNumberTextBox.Text;

            if (!string.IsNullOrWhiteSpace(identificationNumber))
            {
                // Check if the identification number already exists in donations
                Donation existingDonation = Donations.FirstOrDefault(d => d.IdentificationNumber == identificationNumber);

                if (existingDonation != null)
                {
                    // If exists, populate full name and blood type fields
                    FullNameTextBox.Text = existingDonation.FullName;
                    BloodTypeComboBox.SelectedItem = BloodTypeComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == existingDonation.BloodType);

                    // Disable full name and blood type fields
                    FullNameTextBox.IsEnabled = false;
                    BloodTypeComboBox.IsEnabled = false;
                }
                else
                {
                    // If not exists, clear fields and enable them
                    FullNameTextBox.Text = string.Empty;
                    BloodTypeComboBox.SelectedItem = null;
                    DonationDatePicker.SelectedDate = null;

                    FullNameTextBox.IsEnabled = true;
                    BloodTypeComboBox.IsEnabled = true;
                    DonationDatePicker.IsEnabled = true;
                }
            }
        }

        private void GenerateIDNumberButton_Click(object sender, RoutedEventArgs e)
        {
            IdentificationNumberTextBox.Text = Util.GetRandomIsraeliIDNumber();
        }
    }
}