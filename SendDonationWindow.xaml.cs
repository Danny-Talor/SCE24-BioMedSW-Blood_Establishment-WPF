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
            int requestedAmount = int.TryParse(RequestedAmountTextBox.Text, out int amount) ? amount : 0;

            if (!string.IsNullOrEmpty(requestedBloodType) && requestedAmount > 0)
            {
                string compatibleBloodTypes = Util.GetCompatibleBloodTypes(requestedBloodType);
                string recommendedBloodType = Util.GetRecommendedBloodType(requestedBloodType, requestedAmount, Donations);
                CompatibleBloodTypesTextBlock.Text = compatibleBloodTypes;
                RecommendedBloodTypeTextBlock.Text = recommendedBloodType;
            }
            else
            {
                RecommendedBloodTypeTextBlock.Text = string.Empty;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}