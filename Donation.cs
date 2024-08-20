using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void RemoveDonations(int count)
        {
            if (count >= DonationCount)
            {
                DonationCount = 0;
                DonationDates.Clear();
            }
            else
            {
                DonationCount -= count;
                DonationDates = DonationDates.OrderBy(d => d).Skip(count).ToList();
            }
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
}
