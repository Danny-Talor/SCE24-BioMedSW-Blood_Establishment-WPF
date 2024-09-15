using DocumentFormat.OpenXml.Bibliography;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    // Main window class for the WPF application
    public partial class MainWindow : Window
    {
        // Application Data
        public ApplicationData ApplicationData { get; set; }

        // Observable collections for donations and blood type totals
        public ObservableCollection<Donation> Donations { get; set; }
        public ObservableCollection<BloodTotal> BloodTotals { get; set; }

        // Strings
        private const string SearchBarPlaceholderText = "Search...";
        private string CurrentUser = "N/A";

        // Constructor initializes components and collections
        public MainWindow(ApplicationData appData, string username, int userRole)
        {
            InitializeComponent();
            this.ApplicationData = appData;

            if (userRole != (int)Util.UserRole.ADMINISTRATOR)
            {
                PopulateTableButton.Visibility = Visibility.Collapsed;
                UserManagementButton.Visibility = Visibility.Collapsed;
            }

            Donations = new ObservableCollection<Donation>();
            BloodTotals = new ObservableCollection<BloodTotal> // Initialize BloodTotals with default values for each blood type
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

            // Load existing donations if exists
            LoadDonations();

            // Handle window closing event
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
            MessageBoxResult result = MessageBox.Show("Click Yes to populate one of each blood type, Click No to populate randomly",
                                                     "Populate",
                                                     MessageBoxButton.YesNoCancel,
                                                     MessageBoxImage.Warning);
            Random random = new Random();
            int donationCount = 0;
            string[] bloodTypes = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            string[] fullNames = { "John Doe", "Jane Smith", "Michael Johnson", "Emily Brown", "Robert Williams" };

            if (result == MessageBoxResult.Yes)
            {
                // Populate one donation for each blood type
                foreach (var bloodType in bloodTypes)
                {
                    string identificationNumber = Util.GetRandomIsraeliIDNumber();
                    string fullName = fullNames[random.Next(0, fullNames.Length)];
                    DateTime date = DateTime.Now.AddDays(-random.Next(1, 100)); // Random date within the last 100 days
                    Donation newDonation = new Donation(fullName, identificationNumber, bloodType, 1, new List<DateTime> { date });
                    Donations.Add(newDonation);
                    // Add new donation to application data
                    ApplicationData.Donations.Add(newDonation);
                    // Log the new donation
                    var donationLog = new DonationLog
                    {
                        FullName = newDonation.FullName,
                        IdentificationNumber = newDonation.IdentificationNumber,
                        BloodType = newDonation.BloodType,
                        DonationDate = newDonation.DonationDates.Last(),
                        RegisteredBy = CurrentUser
                    };
                    ApplicationData.Logs.Donations.Add(donationLog);
                }
            }
            else if (result == MessageBoxResult.No)
            {
                // Generate 10 random donations
                for (int i = 0; i < 10; i++)
                {
                    string identificationNumber = Util.GetRandomIsraeliIDNumber();
                    string fullName = fullNames[random.Next(0, fullNames.Length)];
                    string bloodType = bloodTypes[random.Next(0, bloodTypes.Length)];

                    List<DateTime> donationDates = new List<DateTime>();
                    int numDates = random.Next(1, 5); // Randomly choose 1 to 4 dates
                    for (int j = 0; j < numDates; j++)
                    {
                        DateTime date = DateTime.Now.AddDays(-random.Next(1, 100)); // Random date within the last 100 days
                        donationDates.Add(date);
                        donationCount++;
                    }

                    Donation newDonation = new Donation(fullName, identificationNumber, bloodType, donationCount, donationDates);
                    Donations.Add(newDonation);
                    // Add new donation to application data
                    ApplicationData.Donations.Add(newDonation);

                    // Log each date as a separate donation log entry
                    foreach (var date in donationDates)
                    {
                        var donationLog = new DonationLog
                        {
                            FullName = newDonation.FullName,
                            IdentificationNumber = newDonation.IdentificationNumber,
                            BloodType = newDonation.BloodType,
                            DonationDate = date,
                            RegisteredBy = CurrentUser
                        };
                        ApplicationData.Logs.Donations.Add(donationLog);
                    }

                    // Reset donationCount for next iteration
                    donationCount = 0;
                }
            }
            else
            {
                // User clicked Cancel or closed the MessageBox
                return;
            }

            // Refresh DataGrid
            DonationsDataGrid.Items.Refresh();

            // Recalculate blood totals
            UpdateBloodTotals();

            // Update status message
            StatusTextBlock.Text = "Table populated with data.";

            // Save application data
            ApplicationData.SaveApplicationData(ApplicationData);
        }

        // Event handler for the "Send donation" button (not implemented yet)
        private void SendDonationButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new SendDonationWindow(Donations, CurrentUser, ApplicationData);
            registerWindow.ShowDialog();

            // Refresh data after sending donation
            DonationsDataGrid.Items.Refresh();
            UpdateBloodTotals();
        }

        // Event handler for the "Mass Casualty Incident" button
        private void MassCasualtyIncidentButton_Click(object sender, RoutedEventArgs e)
        {
            var oNegativeDonations = Donations.Where(d => d.BloodType == "O-").ToList();
            if (oNegativeDonations.Count > 0)
            {
                // Confirmation message box
                MessageBoxResult result = MessageBox.Show("You are about to send all available O- blood type donations to the mass casualty incident. Continue?",
                                                          "Mass Casualty Incident",
                                                          MessageBoxButton.OKCancel,
                                                          MessageBoxImage.Warning);

                if (result == MessageBoxResult.OK)
                {
                    // Log the amount of O- blood to be sent
                    int amountSent = oNegativeDonations.Sum(d => d.DonationCount);

                    // Delete all O- donations from the collection
                    foreach (var donation in oNegativeDonations)
                    {
                        Donations.Remove(donation);
                    }

                    // Create a new MCI log entry
                    var mciLog = new MCILog
                    {
                        AmountSent = amountSent,
                        Timestamp = DateTime.Now,
                        User = CurrentUser
                    };

                    // Add MCI log to the application data
                    ApplicationData.Logs.MCIs.Add(mciLog);

                    // Save application data
                    ApplicationData.SaveApplicationData(ApplicationData);

                    // Refresh DonationsDataGrid
                    DonationsDataGrid.Items.Refresh();

                    // Update blood totals
                    UpdateBloodTotals();

                    // Update status message
                    StatusTextBlock.Text = "MCI completed.";
                }
            }
            else
            {
                MessageBox.Show("No O- blood to send!");
            }
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
                ApplicationData.Donations.Add(donation);
            }

            // Log the new donation
            var donationLog = new DonationLog
            {
                FullName = donation.FullName,
                IdentificationNumber = donation.IdentificationNumber,
                BloodType = donation.BloodType,
                DonationDate = donation.DonationDates.Last(),
                RegisteredBy = CurrentUser
            };
            ApplicationData.Logs.Donations.Add(donationLog);

            // Refresh DataGrids 
            DonationsDataGrid.Items.Refresh();

            // Recalculate blood totals
            UpdateBloodTotals();

            // Save donation
            ApplicationData.SaveApplicationData(ApplicationData);
        }

        // Event handler to show donation dates for a specific donation
        private void ShowDonationDates_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Donation donation)
            {
                if (donation.DonationDates.Any())
                {
                    // Sort donation dates by date
                    var sortedDates = donation.DonationDates.OrderBy(d => d).ToList();

                    // Create formatted string with sorted dates
                    var datesString = string.Join("\n", sortedDates.Select(d => d.ToShortDateString()));

                    // Show MessageBox with sorted donation dates
                    MessageBox.Show($"Donation dates for {donation.FullName}:\n\n{datesString}", "Donation Dates");
                }
                else
                {
                    MessageBox.Show($"No donation dates available for {donation.FullName}", "Donation Dates");
                }
            }
        }

        // Method to load donations from XML file
        private void LoadDonations()
        {
            try
            {
                // Populate ObservableCollection with deserialized donations
                Donations = new ObservableCollection<Donation>(ApplicationData.Donations);
                DonationsDataGrid.ItemsSource = Donations;

                // Update status message on successful load
                StatusTextBlock.Text = "Donations loaded successfully.";
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = "Error loading donations.";
            }

            // Calculate and update blood type totals
            UpdateBloodTotals();
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
            ApplicationData.SaveApplicationData(ApplicationData);
        }

        // Method to calculate total amounts for each blood type
        private void UpdateBloodTotals()
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

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a new export log entry
                var exportLog = new ExportLog
                {
                    Timestamp = DateTime.Now,
                    User = CurrentUser
                };

                // Add MCI log to the application data
                ApplicationData.Logs.Exports.Add(exportLog);

                // Save application data
                ApplicationData.SaveApplicationData(ApplicationData);

                Util.ExportLogs();

                StatusTextBlock.Text = "Logs exported successfuly.";

                MessageBoxResult result = MessageBox.Show("Export successful! Would you like to view the file?",
                                                     "Exported logs",
                                                     MessageBoxButton.YesNo,
                                                     MessageBoxImage.Information);
                if (result == MessageBoxResult.Yes)
                {
                    // Open the Excel file with the default application
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = Util.GetDataFilePath(Util.DataType.EXPORTDATA),
                        UseShellExecute = true // This ensures the file opens with its associated application
                    });
                }

            }
            catch (ApplicationException ex)
            {
                MessageBox.Show(ex.Message, "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Delete new export log entry
                ApplicationData.Logs.Exports.RemoveAt(ApplicationData.Logs.Exports.Count - 1);

                // Save application data
                ApplicationData.SaveApplicationData(ApplicationData);
            }
        }

        private void UserManagementButton_Click(object sender, RoutedEventArgs e)
        {
            return;
        }
    }
}