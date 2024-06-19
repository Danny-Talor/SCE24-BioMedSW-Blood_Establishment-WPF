using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    // Represents a single donation entry
    public class Donation
    {
        // Properties of a donation
        public string FullName { get; set; }
        public string IdentificationNumber { get; set; }
        public string BloodType { get; set; }
        public int DonationCount { get; set; }
        public List<DateTime> DonationDates { get; set; }

        // Default constructor initializes properties
        public Donation()
        {
            FullName = string.Empty;
            IdentificationNumber = string.Empty;
            BloodType = string.Empty;
            DonationCount = 1; // Default donation count is 1
            DonationDates = new List<DateTime>(); // Initialize list of donation dates
        }

        // Parameterized constructor to set properties
        public Donation(string FullName, string IdentificationNumber, string BloodType, int Amount, List<DateTime> DonationDates)
        {
            this.FullName = FullName;
            this.IdentificationNumber = IdentificationNumber;
            this.BloodType = BloodType;
            this.DonationCount = Amount;
            this.DonationDates = DonationDates;
        }
    }

    // Represents aggregated total amounts of each blood type
    public class BloodTotal
    {
        public string BloodType { get; set; }
        public int TotalAmount { get; set; }

        // Constructor to initialize blood type and total amount
        public BloodTotal(string bloodType, int totalAmount)
        {
            BloodType = bloodType;
            TotalAmount = totalAmount;
        }
    }

    // Serializable class to hold list of donations
    [Serializable]
    public class DonationData
    {
        public List<Donation> Donations { get; set; }

        // Default constructor initializes donations list
        public DonationData()
        {
            Donations = new List<Donation>();
        }
    }

    // Main window class for the WPF application
    public partial class MainWindow : Window
    {
        // Observable collections for donations and blood type totals
        public ObservableCollection<Donation> Donations { get; set; }
        public ObservableCollection<BloodTotal> BloodTotals { get; set; }
        private const string SearchBarPlaceholderText = "Search...";

        // Constructor initializes components and collections
        public MainWindow()
        {
            InitializeComponent();
            Donations = new ObservableCollection<Donation>();
            // Initialize BloodTotals with default values for each blood type
            BloodTotals = new ObservableCollection<BloodTotal>
            {
                new BloodTotal("A-", 0),
                new BloodTotal("A+", 0),
                new BloodTotal("AB-", 0),
                new BloodTotal("AB+", 0),
                new BloodTotal("B-", 0),
                new BloodTotal("B+", 0),
                new BloodTotal("O-", 0),
                new BloodTotal("O+", 0)
            };
            // Set data sources for DataGrids
            DonationsDataGrid.ItemsSource = Donations;
            TotalBloodDataGrid.ItemsSource = BloodTotals;
            // Load existing donations and handle window closing event
            LoadDonations();
            Closing += MainWindow_Closing;
        }

        // Event handler for registering a new donation
        private void RegisterDonation_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterDonationWindow(Donations);
            registerWindow.DonationSubmitted += RegisterWindow_DonationSubmitted;
            registerWindow.ShowDialog();
        }

        // Event handler for populating the table with random data
        private void PopulateTable_Click(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int DonationCount = 0;

            // Generate 10 random donations
            for (int i = 0; i < 10; i++)
            {
                // Generate random valid Israeli identification number
                string identificationNumber = random.Next(100000000, 1000000000).ToString();
                while (!Util.IsValidIsraeliIDNumber(identificationNumber))
                {
                    identificationNumber = random.Next(100000000, 1000000000).ToString();
                }

                // Generate random full name
                string[] fullNames = { "John Doe", "Jane Smith", "Michael Johnson", "Emily Brown", "Robert Williams" };
                string fullName = fullNames[random.Next(0, fullNames.Length)];

                // Generate random blood type
                string[] bloodTypes = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
                string bloodType = bloodTypes[random.Next(0, bloodTypes.Length)];

                // Generate random donation dates
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

                // Reset DonationCount for next iteration
                DonationCount = 0;
            }

            // Refresh DataGrid
            DonationsDataGrid.Items.Refresh();

            // Recalculate blood totals
            CalculateBloodTotals();

            // Update status message
            StatusTextBlock.Text = "Table populated with random data.";
        }

        // Event handler for the "Send donation" button (not implemented yet)
        private void SendDonationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("In development", "Send donation");
        }

        // Event handler for the "Mass Casualty Incident" button (not implemented yet)
        private void MassCasualtyIncidentButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("In development", "Mass Casualty Incident");
        }

        // Event handler for donation submission from RegisterDonationWindow
        private void RegisterWindow_DonationSubmitted(object sender, Donation donation)
        {
            // Check if donation already exists
            var existingDonation = Donations.FirstOrDefault(d => d.IdentificationNumber == donation.IdentificationNumber);
            if (existingDonation != null)
            {
                // Increment donation count and add new donation date
                existingDonation.DonationCount++;
                existingDonation.DonationDates.Add(donation.DonationDates[0]);
            }
            else
            {
                // Add new donation to ObservableCollection
                Donations.Add(donation);
            }

            // Refresh DataGrids and recalculate blood totals
            DonationsDataGrid.Items.Refresh();
            CalculateBloodTotals();
            // Save donations to file
            SaveDonations();
        }

        // Event handler to show donation dates for a specific donation
        private void ShowDonationDates_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Donation donation)
            {
                // Sort donation dates by date
                var sortedDates = donation.DonationDates.OrderBy(d => d).ToList();

                // Create formatted string with sorted dates
                var datesString = string.Join("\n", sortedDates.Select(d => d.ToShortDateString()));

                // Show MessageBox with sorted donation dates
                MessageBox.Show($"Donation dates for {donation.FullName}:\n\n{datesString}", "Donation Dates");
            }
        }

        // Method to save donations to XML file
        private void SaveDonations()
        {
            try
            {
                // Get path to AppData folder
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(appDataFolder, "donations.xml");

                // Create DonationData object with current donations
                DonationData donationData = new DonationData
                {
                    Donations = new List<Donation>(Donations)
                };

                // Serialize DonationData object to XML and save to file
                XmlSerializer serializer = new XmlSerializer(typeof(DonationData));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fileStream, donationData);
                }

                // Update status message on successful save
                StatusTextBlock.Text = "Donations updated successfully.";
            }
            catch (Exception ex)
            {
                // Show error message if saving fails
                MessageBox.Show($"An error occurred while saving donations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Error saving donations.";
            }
        }

        // Method to load donations from XML file
        private void LoadDonations()
        {
            try
            {
                // Get path to AppData folder
                string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string filePath = Path.Combine(appDataFolder, "donations.xml");

                // If donations file exists, deserialize it
                if (File.Exists(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(DonationData));
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        DonationData donationData = (DonationData)serializer.Deserialize(fileStream);
                        // Populate ObservableCollection with deserialized donations
                        Donations = new ObservableCollection<Donation>(donationData.Donations);
                        DonationsDataGrid.ItemsSource = Donations;
                    }

                    // Update status message on successful load
                    StatusTextBlock.Text = "Donations loaded successfully.";
                }
            }
            catch (Exception ex)
            {
                // Show error message if loading fails
                MessageBox.Show($"An error occurred while loading donations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Error loading donations.";
            }

            // Calculate and update blood type totals
            CalculateBloodTotals();
        }

        // Event handler for search button click
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();
            // Filter donations based on search text
            var filteredDonations = Donations.Where(d =>
                d.FullName.ToLower().Contains(searchText) ||
                d.IdentificationNumber.ToLower().Contains(searchText) ||
                d.BloodType.ToLower().Contains(searchText)
            ).ToList();

            // Update DataGrid with filtered donations
            DonationsDataGrid.ItemsSource = filteredDonations;
        }

        // Event handler for search text box text changed
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Reset DataGrid to show all donations if search text is empty
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text) || SearchTextBox.Text == SearchBarPlaceholderText)
            {
                DonationsDataGrid.ItemsSource = Donations;
            }
        }

        // Event handler when search text box gains focus
        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Clear placeholder text when search box gains focus
            if (SearchTextBox.Text == SearchBarPlaceholderText)
            {
                SearchTextBox.Text = string.Empty;
                SearchTextBox.Foreground = SystemColors.ControlTextBrush;
            }
        }

        // Event handler when search text box loses focus
        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Restore placeholder text if search box is empty and loses focus
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SetPlaceholderText();
            }
        }

        // Method to set placeholder text in search text box
        private void SetPlaceholderText()
        {
            SearchTextBox.Text = SearchBarPlaceholderText;
            SearchTextBox.Foreground = SystemColors.GrayTextBrush;
        }

        // Event handler when main window is closing
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save donations when window is closing
            SaveDonations();
        }

        // Method to calculate total amounts for each blood type
        private void CalculateBloodTotals()
        {
            // Reset total amounts for each blood type
            foreach (var bloodTotal in BloodTotals)
            {
                bloodTotal.TotalAmount = 0;
            }

            // Calculate total amounts based on current donations
            foreach (var donation in Donations)
            {
                var bloodTotal = BloodTotals.FirstOrDefault(bt => bt.BloodType == donation.BloodType);
                if (bloodTotal != null)
                {
                    bloodTotal.TotalAmount += donation.DonationCount;
                }
            }

            // Refresh total blood data grid to reflect updated totals
            TotalBloodDataGrid.Items.Refresh();
        }
    }
}