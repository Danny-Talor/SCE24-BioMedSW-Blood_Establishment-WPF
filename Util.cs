﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    // Utility class for common methods
    internal class Util
    {
        public enum DataType
        {
            APPDATA,
            EXPORTDATA,
        }

        // Define default application data file names
        public const string ApplicationDataFileName = "SCE24-BioMedSW-BECS-data.xml";
        public const string ExportDataFileName = "SCE24-BioMedSW-BECS-exported.xlsx";

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
        public static string GetDataFilePath(DataType type)
        {
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); // Get path to AppData folder
            if (type == DataType.APPDATA)
            {
                return Path.Combine(appDataFolder, ApplicationDataFileName); // Return the full path to the application data file
            }
            else if (type == DataType.EXPORTDATA)
            {
                return Path.Combine(appDataFolder, ExportDataFileName); // Return the full path to the application data file
            }
            else
            {
                return "";
            }
        }

        public static void ExportLogs()
        {
            try
            {
                // Load the XML data
                XDocument xmlDoc = XDocument.Load(GetDataFilePath(DataType.APPDATA));
                XElement logsElement = xmlDoc.Element("ApplicationData")?.Element("Logs");

                if (logsElement == null)
                {
                    throw new Exception("Logs element not found in the XML.");
                }

                // Create a new Excel workbook
                using (var workbook = new XLWorkbook())
                {
                    // Export Donations logs
                    var donationsLogs = logsElement.Element("Donations")?.Elements("DonationLog");
                    if (donationsLogs != null && donationsLogs.Any())
                    {
                        var donationsSheet = workbook.Worksheets.Add("Donations");
                        int row = 1;
                        foreach (var log in donationsLogs)
                        {
                            donationsSheet.Cell(row, 1).Value = log.Element("FullName")?.Value;
                            donationsSheet.Cell(row, 2).Value = log.Element("IdentificationNumber")?.Value;
                            donationsSheet.Cell(row, 3).Value = log.Element("BloodType")?.Value;
                            donationsSheet.Cell(row, 4).Value = TimestampStringNormalize(log.Element("DonationDate")?.Value);
                            donationsSheet.Cell(row, 5).Value = log.Element("RegisteredBy")?.Value;
                            row++;
                        }
                    }

                    // Export Blood Transfers logs
                    var bloodTransfersLogs = logsElement.Element("BloodTransfers")?.Elements("BloodTransferLog");
                    if (bloodTransfersLogs != null && bloodTransfersLogs.Any())
                    {
                        var bloodTransfersSheet = workbook.Worksheets.Add("Blood Transfers");
                        int row = 1;
                        foreach (var log in bloodTransfersLogs)
                        {
                            bloodTransfersSheet.Cell(row, 1).Value = log.Element("Donor")?.Value;
                            bloodTransfersSheet.Cell(row, 2).Value = log.Element("ID")?.Value;
                            bloodTransfersSheet.Cell(row, 3).Value = log.Element("BloodType")?.Value;
                            bloodTransfersSheet.Cell(row, 4).Value = log.Element("RequestedBloodType")?.Value;
                            bloodTransfersSheet.Cell(row, 5).Value = log.Element("RequestedAmount")?.Value;
                            bloodTransfersSheet.Cell(row, 6).Value = log.Element("TransferredAmount")?.Value;
                            bloodTransfersSheet.Cell(row, 7).Value = log.Element("RequestedDepartment")?.Value;
                            bloodTransfersSheet.Cell(row, 8).Value = TimestampStringNormalize(log.Element("Timestamp")?.Value);
                            bloodTransfersSheet.Cell(row, 9).Value = log.Element("User")?.Value;
                            row++;
                        }
                    }

                    // Export Mass Casualty Incidents logs
                    var mciLogs = logsElement.Element("MCIs")?.Elements("MCILog");
                    if (mciLogs != null && mciLogs.Any())
                    {
                        var mciSheet = workbook.Worksheets.Add("Mass Casualty Incidents");

                        int row = 1;
                        foreach (var log in mciLogs)
                        {
                            mciSheet.Cell(row, 1).Value = log.Element("AmountSent")?.Value;
                            mciSheet.Cell(row, 2).Value = TimestampStringNormalize(log.Element("Timestamp")?.Value);
                            mciSheet.Cell(row, 3).Value = log.Element("User")?.Value;
                            row++;
                        }
                    }

                    // Export export logs
                    var exportLogs = logsElement.Element("Exports")?.Elements("ExportLog");
                    if (exportLogs != null && exportLogs.Any())
                    {
                        var exportsSheet = workbook.Worksheets.Add("Logged Exports");

                        int row = 1;
                        foreach (var log in exportLogs)
                        {
                            exportsSheet.Cell(row, 1).Value = TimestampStringNormalize(log.Element("Timestamp")?.Value);
                            exportsSheet.Cell(row, 2).Value = log.Element("User")?.Value;
                            row++;
                        }
                    }

                    // Save the workbook to the specified path
                    workbook.SaveAs(GetDataFilePath(DataType.EXPORTDATA));
                }
            }
            catch (IOException ioEx)
            {
                throw new ApplicationException($"Please ensure the file is not open and try again.", ioEx);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"An error occurred: {ex.Message}", ex);
            }

        }


        public static string TimestampStringNormalize(string timestamp)
        {
            DateTime value;
            if (DateTime.TryParse(timestamp,out value)) return value.ToString("dd.MM.yyyy HH:mm:ss");
            else return "?";

        }
    }
}
