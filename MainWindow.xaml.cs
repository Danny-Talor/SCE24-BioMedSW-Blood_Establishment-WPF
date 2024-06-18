using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Donation> Donations { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Donations = new ObservableCollection<Donation>();
            DonationsDataGrid.ItemsSource = Donations;
        }

        private void RegisterDonation_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterDonationWindow(Donations);
            registerWindow.DonationSubmitted += RegisterWindow_DonationSubmitted;
            registerWindow.ShowDialog();
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
    }
}