using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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

        // Blood type rarity percentages (higher percentage means more common)
        static readonly Dictionary<string, double> bloodTypeRarity = new Dictionary<string, double>
    {
        {"A+", 0.34},
        {"O+", 0.32},
        {"B+", 0.17},
        {"AB+", 0.07},
        {"A-", 0.04},
        {"O-", 0.03},
        {"B-", 0.02},
        {"AB-", 0.01},
    };

        public static string GetCompatibleBloodTypes(string requestedBloodType)
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

            // Determine the recommended blood type based on availability, requested amount, and rarity
            string recommendedBloodType = null;
            double maxScore = 0;

            foreach (var bloodType in compatibleDonorTypes)
            {
                if (totalAmounts.ContainsKey(bloodType))
                {
                    int availableAmount = totalAmounts[bloodType];

                    // Calculate a score based on availability and rarity percentage
                    double rarityPercentage = bloodTypeRarity[bloodType];
                    double score = availableAmount * rarityPercentage;

                    if (availableAmount >= requestedAmount && score > maxScore)
                    {
                        recommendedBloodType = bloodType;
                        maxScore = score;
                    }
                }
            }

            return recommendedBloodType ?? "No recommended blood type found or not enough blood in stock.";
        }
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

        // Generate random valid Israeli identification number
        public static string GetRandomIsraeliIDNumber()
        {
            Random random = new Random();
            string identificationNumber = random.Next(100000000, 1000000000).ToString();
            while (!Util.IsValidIsraeliIDNumber(identificationNumber))
            {
                identificationNumber = random.Next(100000000, 1000000000).ToString();
            }
            return identificationNumber;
        }

        // turn string into TitleCase
        public static string ToTitleCase(string input)
        {
            TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
            string[] words = input.ToLower().Split(' ');

            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = textInfo.ToTitleCase(words[i]);
                }
            }

            return string.Join(" ", words);
        }
    }
}
