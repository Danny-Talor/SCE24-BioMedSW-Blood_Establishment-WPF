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
    public partial class SendDonationWindow : Window
    {
        private readonly ObservableCollection<Donation> Donations;

        public SendDonationWindow(ObservableCollection<Donation> donations)
        {
            InitializeComponent();
            Donations = donations;

            RequestedBloodTypeComboBox.SelectionChanged += RequestedBloodTypeComboBox_SelectionChanged;
            RequestedAmountTextBox.TextChanged += AmountTextBox_TextChanged;
        }

        private void RequestedBloodTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRecommendedBloodType();
        }

        private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateRecommendedBloodType();
        }

        private void UpdateRecommendedBloodType()
        {
            string requestedBloodType = (RequestedBloodTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (!string.IsNullOrEmpty(requestedBloodType))
            {
                string compatibleBloodTypes = Util.GetCompatibleBloodTypes(requestedBloodType);
                CompatibleBloodTypesTextBlock.Text = compatibleBloodTypes;

                UpdateBloodTypeToSendComboBox(compatibleBloodTypes);

                if (int.TryParse(RequestedAmountTextBox.Text, out int requestedAmount) && requestedAmount > 0)
                {
                    string recommendedBloodType = Util.GetRecommendedBloodType(requestedBloodType, requestedAmount, Donations);
                    RecommendedBloodTypeTextBlock.Text = recommendedBloodType;

                    if (recommendedBloodType == "No recommended blood type found or not enough blood in stock.")
                    {
                        SendButton.IsEnabled = false;
                    }
                    else
                    {
                        SendButton.IsEnabled = true;
                        BloodTypeToSendComboBox.SelectedItem = BloodTypeToSendComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == recommendedBloodType);
                    }
                }
                else
                {
                    RecommendedBloodTypeTextBlock.Text = string.Empty;
                    SendButton.IsEnabled = true;
                }
            }
            else
            {
                CompatibleBloodTypesTextBlock.Text = string.Empty;
                RecommendedBloodTypeTextBlock.Text = string.Empty;
                BloodTypeToSendComboBox.Items.Clear();
                SendButton.IsEnabled = true;
            }
        }

        private void UpdateBloodTypeToSendComboBox(string compatibleBloodTypes)
        {
            BloodTypeToSendComboBox.Items.Clear();
            var bloodTypes = compatibleBloodTypes.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var bloodType in bloodTypes)
            {
                BloodTypeToSendComboBox.Items.Add(new ComboBoxItem { Content = bloodType });
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            ClearErrorMessages();

            if (!ValidateInputs())
            {
                return;
            }

            string bloodTypeToSend = (BloodTypeToSendComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            int requestedAmount = int.Parse(RequestedAmountTextBox.Text);
            string department = (DepartmentComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (!TrySendBlood(bloodTypeToSend, requestedAmount))
            {
                MessageBox.Show("Out of blood for the requested type", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Successfully sent {requestedAmount} units of {bloodTypeToSend} blood to {department}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
        private bool ValidateInputs()
        {
            bool isValid = true;

            if (RequestedBloodTypeComboBox.SelectedItem == null)
            {
                BloodTypeErrorMessage.Text = "Please select a blood type";
                isValid = false;
            }

            if (!int.TryParse(RequestedAmountTextBox.Text, out int amount) || amount <= 0)
            {
                AmountErrorMessage.Text = "Please enter a valid amount";
                isValid = false;
            }

            if (DepartmentComboBox.SelectedItem == null)
            {
                DepartmentErrorMessage.Text = "Please select a department";
                isValid = false;
            }

            if (BloodTypeToSendComboBox.SelectedItem == null)
            {
                BloodTypeToSendErrorMessage.Text = "Please select a blood type to send";
                isValid = false;
            }
            else
            {
                string requestedBloodType = (RequestedBloodTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string bloodTypeToSend = (BloodTypeToSendComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
                string compatibleBloodTypes = Util.GetCompatibleBloodTypes(requestedBloodType);

                if (!compatibleBloodTypes.Contains(bloodTypeToSend))
                {
                    MessageBox.Show($"The selected blood type to send ({bloodTypeToSend}) is not compatible with the requested blood type ({requestedBloodType})", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    isValid = false;
                }
            }

            return isValid;
        }

        private void ClearErrorMessages()
        {
            BloodTypeErrorMessage.Text = string.Empty;
            AmountErrorMessage.Text = string.Empty;
            DepartmentErrorMessage.Text = string.Empty;
            BloodTypeToSendErrorMessage.Text = string.Empty;
        }

        private bool TrySendBlood(string bloodType, int requestedAmount)
        {
            var compatibleDonations = Donations.Where(d => d.BloodType == bloodType).OrderBy(d => d.DonationDates.Min()).ToList();
            int availableAmount = compatibleDonations.Sum(d => d.DonationCount);

            if (availableAmount >= requestedAmount)
            {
                int remainingAmount = requestedAmount;
                foreach (var donation in compatibleDonations.ToList()) // Use ToList() to avoid modifying collection during iteration
                {
                    if (donation.DonationCount <= remainingAmount)
                    {
                        remainingAmount -= donation.DonationCount;
                        Donations.Remove(donation);
                    }
                    else
                    {
                        int removedDates = remainingAmount;
                        donation.DonationCount -= remainingAmount;

                        // Remove the oldest donation dates
                        donation.DonationDates = donation.DonationDates.OrderBy(d => d).Skip(removedDates).ToList();

                        remainingAmount = 0;
                    }

                    if (remainingAmount == 0)
                    {
                        break;
                    }
                }
                return true;
            }
            else
            {
                string nextCompatibleBloodType = Util.GetRecommendedBloodType(bloodType, requestedAmount, Donations);
                if (nextCompatibleBloodType != "No recommended blood type found or not enough blood in stock.")
                {
                    MessageBoxResult result = MessageBox.Show($"Not enough {bloodType} blood available. Would you like to use {nextCompatibleBloodType} instead?", "Alternative Blood Type", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        BloodTypeToSendComboBox.SelectedItem = BloodTypeToSendComboBox.Items.Cast<ComboBoxItem>().FirstOrDefault(item => item.Content.ToString() == nextCompatibleBloodType);
                        return TrySendBlood(nextCompatibleBloodType, requestedAmount);
                    }
                }
                return false;
            }
        }
    }
}