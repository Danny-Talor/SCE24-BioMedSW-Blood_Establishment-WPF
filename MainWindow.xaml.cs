using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    public class Donation
    {
        public string FullName { get; set; }
        public string IdentificationNumber { get; set; }
        public string BloodType { get; set; }
        public int DonationCount { get; set; }
        public List<DateTime> DonationDates { get; set; }

        public Donation()
        {
            FullName = String.Empty;
            IdentificationNumber = String.Empty;
            BloodType = String.Empty;
            DonationCount = 0;
            DonationDates = new List<DateTime>();
        }
        public Donation(string FullName, string IdentificationNumber, string BloodType, int Amount, List<DateTime> DonationDates)
        {
            this.FullName = FullName;
            this.IdentificationNumber = IdentificationNumber;
            this.BloodType = BloodType;
            this.DonationCount = Amount;
            this.DonationDates = DonationDates;
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

        private void PopulateTable_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int DonationCount = 0;

            // Generate random donations
            for (int i = 0; i < 10; i++) // Generating 10 random donations as an example
            {
                // Generate random 9-digit identification number
                string identificationNumber = random.Next(100000000, 1000000000).ToString();

                // Generate random full name (for simplicity, using a basic list of names)
                string[] fullNames = { "John Doe", "Jane Smith", "Michael Johnson", "Emily Brown", "Robert Williams" };
                string fullName = fullNames[random.Next(0, fullNames.Length)];

                // Generate random blood type (for simplicity, using a basic list of blood types)
                string[] bloodTypes = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                string bloodType = bloodTypes[random.Next(0, bloodTypes.Length)];

                // Generate random donation dates and with each date add to the amount of donations
                List<DateTime> donationDates = new List<DateTime>();
                int numDates = random.Next(1, 4); // Randomly choose 1 to 3 dates
                for (int j = 0; j < numDates; j++)
                {
                    DateTime date = DateTime.Now.AddDays(-random.Next(1, 100)); // Random date within the last 100 days
                    donationDates.Add(date);
                    DonationCount++;
                }

                // Create new Donation object
                Donation newDonation = new Donation(fullName, identificationNumber, bloodType, DonationCount, donationDates);

                // Add to ObservableCollection
                Donations.Add(newDonation);

                // Reset
                DonationCount = 0;
            }

            // Refresh DataGrid
            DonationsDataGrid.Items.Refresh();

            // Optionally, update status
            StatusTextBlock.Text = "Table populated with random data.";
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
                existingDonation.DonationCount++;
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
                // Sort donationDates by date
                var sortedDates = donation.DonationDates.OrderBy(d => d).ToList();

                // Create formatted string with sorted dates
                var datesString = string.Join("\n", sortedDates.Select(d => d.ToShortDateString()));

                // Show MessageBox with sorted donation dates
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