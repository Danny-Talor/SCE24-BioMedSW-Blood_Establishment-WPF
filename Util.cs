using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    // Utility class for common methods
    internal class Util
    {
        // Define a dictionary to map recipient blood types to compatible donor blood types
        static readonly Dictionary<string, string[]> donorBloodTypes = new Dictionary<string, string[]>
    {
        {"O-", new[] {"O-"}},
        {"O+", new[] {"O-", "O+"}},
        {"A-", new[] {"O-", "A-"}},
        {"A+", new[] {"O-", "O+", "A-", "A+"}},
        {"B-", new[] {"O-", "B-"}},
        {"B+", new[] {"O-", "O+", "B-", "B+"}},
        {"AB-", new[] {"O-", "A-", "B-", "AB-"}},
        {"AB+", new[] {"O-", "O+", "A-", "A+", "B-", "B+", "AB-", "AB+"}}
    };
        // Method to validate Israeli ID number
        public static bool IsValidIsraeliIDNumber(string id)
        {
            if (id.Length > 9) return false;
            if (id.Length < 9) while (id.Length != 9) id = "0" + id;
            int counter = 0, incNum, i;
            for (i = 0; i < 9; i++, counter += incNum)
            {
                incNum = (id[i] - '0') * ((i % 2) + 1);
                if (incNum > 9) incNum -= 9;
            }
            return (counter % 10 == 0);
        }

        public static string GetRecommendedBloodType(string requestedBloodType)
        {

            // Check if the recipient blood type is valid
            if (!donorBloodTypes.ContainsKey(requestedBloodType))
            {
                return "Invalid recipient blood type";
            }

            // Get the compatible donor blood types for the given recipient blood type
            var compatibleDonorTypes = donorBloodTypes[requestedBloodType];


            // Return a comma-separated string of compatible donor blood types
            return string.Join(", ", compatibleDonorTypes);
        }
        public static string GetRecommendedBloodType(string requestedBloodType, int requestedAmount, ObservableCollection<Donation> donations)
        {


            // Check if the recipient blood type is valid
            if (!donorBloodTypes.ContainsKey(requestedBloodType))
            {
                return "Invalid recipient blood type";
            }

            // Get the compatible donor blood types for the given recipient blood type
            var compatibleDonorTypes = donorBloodTypes[requestedBloodType];

            // Calculate the total amount of each compatible donor blood type in the donations
            var totalAmounts = new Dictionary<string, int>();
            foreach (var donation in donations)
            {
                if (compatibleDonorTypes.Contains(donation.BloodType))
                {
                    if (totalAmounts.ContainsKey(donation.BloodType))
                    {
                        totalAmounts[donation.BloodType] += donation.DonationCount;
                    }
                    else
                    {
                        totalAmounts[donation.BloodType] = donation.DonationCount;
                    }
                }
            }

            // Determine the recommended blood type based on the requested amount and availability
            string recommendedBloodType = null;
            int maxAmount = 0;

            foreach (var bloodType in compatibleDonorTypes)
            {
                if (totalAmounts.ContainsKey(bloodType))
                {
                    int availableAmount = totalAmounts[bloodType];
                    if (availableAmount >= requestedAmount && availableAmount > maxAmount)
                    {
                        recommendedBloodType = bloodType;
                        maxAmount = availableAmount;
                    }
                }
            }

            return recommendedBloodType ?? "No recommended blood type found or not enough blood in stock.";
        }
    }
}
