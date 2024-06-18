using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    public class Donation
    {
        public string FullName { get; set; }
        public string IdentificationNumber { get; set; }
        public string BloodType { get; set; }
        public int Amount { get; set; }
        public List<DateTime> DonationDates { get; set; }

        public Donation()
        {
            Amount = 1;
            DonationDates = new List<DateTime>();
        }
    }

    [Serializable]
    public class DonationData
    {
        public List<Donation> Donations { get; set; }

        public DonationData()
        {
            Donations = new List<Donation>();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Donation> Donations { get; set; }
        private const string SearchBarPlaceholderText = "Search...";

        public MainWindow()
        {
            InitializeComponent();
            Donations = new ObservableCollection<Donation>();
            DonationsDataGrid.ItemsSource = Donations;
            LoadDonations();
            Closing += MainWindow_Closing;
        }

        private void RegisterDonation_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterDonationWindow(Donations);
            registerWindow.DonationSubmitted += RegisterWindow_DonationSubmitted;
            registerWindow.ShowDialog();
        }

        private void SendDonationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("In development", "Send donation");
        }

        private void MassCasualtyIncidentButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("In development", "Mass Casualty Incident");
        }

        private void RegisterWindow_DonationSubmitted(object sender, Donation donation)
        {
            var existingDonation = Donations.FirstOrDefault(d => d.IdentificationNumber == donation.IdentificationNumber);
            if (existingDonation != null)
            {
                existingDonation.Amount++;
                existingDonation.DonationDates.Add(donation.DonationDates[0]);
            }
            else
            {
                Donations.Add(donation);
            }
            DonationsDataGrid.Items.Refresh();
        }

        private void ShowDonationDates_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Donation donation)
            {
                var datesString = string.Join("\n", donation.DonationDates.Select(d => d.ToShortDateString()));
                MessageBox.Show($"Donation dates for {donation.FullName}:\n\n{datesString}", "Donation Dates");
            }
        }

        private void SaveDonations()
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = System.IO.Path.Combine(appDataFolder, "donations.xml");

                DonationData donationData = new DonationData
                {
                    Donations = new List<Donation>(Donations)
                };

                XmlSerializer serializer = new XmlSerializer(typeof(DonationData));

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fileStream, donationData);
                }

                StatusTextBlock.Text = "Donations saved successfully.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while saving donations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Error saving donations.";
            }
        }

        private void LoadDonations()
        {
            try
            {
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = System.IO.Path.Combine(appDataFolder, "donations.xml");

                if (File.Exists(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DonationData));

                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        DonationData donationData = (DonationData)serializer.Deserialize(fileStream);
                        Donations = new ObservableCollection<Donation>(donationData.Donations);
                        DonationsDataGrid.ItemsSource = Donations;
                    }

                    StatusTextBlock.Text = "Donations loaded successfully.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while loading donations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Error loading donations.";
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            var filteredDonations = Donations.Where(d =>
                d.FullName.ToLower().Contains(searchText) ||
                d.IdentificationNumber.ToLower().Contains(searchText) ||
                d.BloodType.ToLower().Contains(searchText)
            ).ToList();

            DonationsDataGrid.ItemsSource = filteredDonations;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text) || SearchTextBox.Text == SearchBarPlaceholderText)
            {
                DonationsDataGrid.ItemsSource = Donations;
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Text == SearchBarPlaceholderText)
            {
                SearchTextBox.Text = string.Empty;
                SearchTextBox.Foreground = SystemColors.ControlTextBrush;
            }
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SetPlaceholderText();
            }
        }

        private void SetPlaceholderText()
        {
            SearchTextBox.Text = SearchBarPlaceholderText;
            SearchTextBox.Foreground = SystemColors.GrayTextBrush;
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveDonations();
        }
    }
}