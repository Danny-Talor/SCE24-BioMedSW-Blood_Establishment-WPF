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
    
    public partial class StatisticsWindow : Window
    {
        ObservableCollection<Donation> Donations;
        ObservableCollection<BloodTotal> BloodTotals { get; set; }
        ObservableCollection<BloodTransferLog> BloodTransfers;

        public StatisticsWindow()
        {
            InitializeComponent();
            Donations = new ObservableCollection<Donation>(ApplicationData.LoadApplicationData().Donations);
            BloodTransfers = new ObservableCollection<BloodTransferLog>(ApplicationData.LoadApplicationData().Logs.BloodTransfers);

            // Populate statistics
            PopulateDonationTrends();
            PopulateBloodTypeDistribution();
            PopulateDonationByAgeGroup();
            PopulateBloodTypeUsage();


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

           UpdateBloodTotals();

           TotalBloodDataGrid.ItemsSource = BloodTotals;
        }

        private void PopulateDonationTrends()
        {
            var trends = GetDonationTrendsByMonth();
            DonationTrendsDataGrid.ItemsSource = trends.Select(t => new { Month = t.Key.ToString("MMM yyyy"), Count = t.Value });
        }

        private void PopulateBloodTypeDistribution()
        {
            var distribution = GetBloodTypeDistributionByDate();
            BloodTypeDistributionDataGrid.ItemsSource = distribution.SelectMany(d => d.Value.Select(b => new { Date = d.Key.ToString("MMM yyyy"), BloodType = b.Key, Count = b.Value }));
        }

        private void PopulateDonationByAgeGroup()
        {
            var ageGroupStats = GetDonationStatisticsByAgeGroup();
            DonationByAgeGroupDataGrid.ItemsSource = ageGroupStats.Select(a => new { AgeGroup = a.Key, TotalDonations = a.Value });
        }

        private void PopulateBloodTypeUsage()
        {
            var usage = GetBloodTypeUsage();
            BloodTypeUsageDataGrid.ItemsSource = usage.Select(u => new { BloodType = u.Key, TotalDonations = u.Value });
        }

        public Dictionary<DateTime, int> GetDonationTrendsByMonth()
        {
            return Donations
                .SelectMany(d => d.DonationDates)
                .GroupBy(date => new DateTime(date.Year, date.Month, 1))
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public Dictionary<DateTime, Dictionary<string, int>> GetBloodTypeDistributionByDate()
        {
            return Donations
                .SelectMany(d => d.DonationDates.Select(date => new { d.BloodType, Date = new DateTime(date.Year, date.Month, 1) }))
                .GroupBy(x => x.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(bg => bg.BloodType).ToDictionary(bg => bg.Key, bg => bg.Count()));
        }

        public Dictionary<string, int> GetDonationStatisticsByAgeGroup()
        {
            return Donations
                .GroupBy(d => GetAgeGroup(Util.GetAge(d.BirthDate)))
                .ToDictionary(g => g.Key, g => g.Sum(d => d.DonationCount));
        }

        private string GetAgeGroup(int age)
        {
            if (age < 23) return "17-22";
            if (age < 29) return "23-28";
            if (age < 35) return "29-34";
            if (age < 41) return "35-40";
            if (age < 47) return "41-46";
            if (age < 63) return "47-62";
            if (age < 66) return "63-65";
            return "65+";
        }

        public Dictionary<string, int> GetDonationFrequency()
        {
            return Donations
                .GroupBy(d => d.IdentificationNumber)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.DonationCount));
        }

        public Dictionary<string, int> GetBloodTypeUsage()
        {
            // Initialize a dictionary to store the blood type usage
            var bloodTypeUsage = new Dictionary<string, int>();

            // Iterate over each blood transfer log entry
            foreach (var transfer in BloodTransfers)
            {
                // If the requested blood type is not already in the dictionary, add it
                if (!bloodTypeUsage.ContainsKey(transfer.RequestedBloodType))
                {
                    bloodTypeUsage[transfer.RequestedBloodType] = 0;
                }

                // Add the transferred amount to the blood type usage
                bloodTypeUsage[transfer.RequestedBloodType] += transfer.TransferredAmount;
            }

            return bloodTypeUsage;
        }

        public double GetDonorRetentionRate()
        {
            var donorCounts = Donations
                .GroupBy(d => d.IdentificationNumber)
                .Select(g => g.Count())
                .ToList();

            int returningDonors = donorCounts.Count(c => c > 1);
            int totalDonors = donorCounts.Count;

            return totalDonors > 0 ? (double)returningDonors / totalDonors * 100 : 0;
        }

        public void UpdateBloodTotals()
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
