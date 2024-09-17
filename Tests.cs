using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SCE24_BioMedSW_Blood_Establishment_WPF
{
    public class Tests
    {
        [Fact]
        public void CanRegisterValidDonation()
        {
            var donation = new Donation
            {
                FullName = "John Doe",
                IdentificationNumber = "123456789",
                BloodType = "O+",
                DonationCount = 1
            };

            Assert.True(donation.IsEligibleForDonation());  // Assuming you have a method to check eligibility
        }
        [Fact]
        public void CanSubmitDonationForm()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("Register Donation Window");

            var nameBox = window.Get<TextBox>("NameTextBox");
            var idBox = window.Get<TextBox>("IDTextBox");
            var bloodTypeComboBox = window.Get<ComboBox>("BloodTypeComboBox");
            var submitButton = window.Get<Button>("SubmitButton");

            nameBox.Text = "John Doe";
            idBox.Text = "123456789";
            bloodTypeComboBox.Select("O+");
            submitButton.Click();

            var successMessage = window.Get<Label>("SuccessMessage");
            Assert.Equal("Donation successfully registered!", successMessage.Text);

            app.Close();
        }
        [Fact]
        public void IneligibleDonationRecentDonation()
        {
            var donation = new Donation
            {
                FullName = "Jane Doe",
                IdentificationNumber = "987654321",
                BloodType = "A+",
                DonationCount = 2,
                DonationDates = new List<DateTime> { DateTime.Now.AddDays(-10) }
            };

            Assert.False(donation.IsEligibleForDonation());
        }
        [Fact]
        public void IneligibleDonationExceedsLimit()
        {
            var donation = new Donation
            {
                FullName = "Mark Smith",
                IdentificationNumber = "123456789",
                BloodType = "B+",
                DonationCount = 7
            };

            Assert.False(donation.IsEligibleForDonation());
        }
        [Fact]
        public void DonationFrequencyIsValid()
        {
            var donation = new Donation
            {
                FullName = "John Doe",
                DonationDates = new List<DateTime>
        {
            DateTime.Now.AddDays(-100),
            DateTime.Now.AddDays(-60),
            DateTime.Now.AddDays(-30)
        }
            };

            Assert.Equal(3, donation.DonationCount);
        }
        [Fact]
        public void CanLogDonationCorrectly()
        {
            var log = new DonationLog
            {
                FullName = "John Doe",
                IdentificationNumber = "123456789",
                BloodType = "O+",
                DateOfDonation = DateTime.Now,
                User = "Admin"
            };

            Assert.NotNull(log);
            Assert.Equal("John Doe", log.FullName);
        }
        [Fact]
        public void CanLogBloodTransfer()
        {
            var transfer = new BloodTransfer
            {
                Donor = "John Doe",
                ID = "123456789",
                BloodType = "O+",
                RequestedBloodType = "O+",
                RequestedAmount = 2,
                RequestedDepartment = "ER",
                Timestamp = DateTime.Now,
                User = "Admin"
            };

            Assert.NotNull(transfer);
            Assert.Equal("O+", transfer.BloodType);
        }
        [Fact]
        public void CannotTransferMoreBloodThanAvailable()
        {
            var transfer = new BloodTransfer
            {
                RequestedAmount = 5,
                AvailableAmount = 3  // Assuming there's a property for available amount
            };

            Assert.False(transfer.CanTransfer());  // Assuming a method to check validity
        }
        [Fact]
        public void CanLogMCIIncident()
        {
            var mci = new MCILog
            {
                AmountSent = 10,
                Timestamp = DateTime.Now,
                User = "Admin"
            };

            Assert.NotNull(mci);
            Assert.Equal(10, mci.AmountSent);
        }
        [Fact]
        public void CanGetBloodTypeUsage()
        {
            var usage = applicationData.logs.bloodTransfers.Where(x => x.BloodType == "O-").Count();

            Assert.True(usage > 0);
        }
        [Fact]
        public void CanCalculateDonorRetentionRate()
        {
            var retentionRate = donationService.CalculateDonorRetention();

            Assert.InRange(retentionRate, 0, 100);  // Assuming percentage
        }
        [Fact]
        public void CanGetDonationTrendsOverTime()
        {
            var trends = donationService.GetDonationTrends(DateTime.Now.AddMonths(-6), DateTime.Now);

            Assert.NotNull(trends);
            Assert.True(trends.Any());
        }
        [Fact]
        public void CanGetBloodTypeDistributionByDate()
        {
            var distribution = donationService.GetBloodTypeDistribution(DateTime.Now.AddMonths(-3), DateTime.Now);

            Assert.NotNull(distribution);
            Assert.True(distribution.ContainsKey("O+"));
        }
        [Fact]
        public void CanOpenRegisterDonationWindow()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("MainWindow");
            var registerButton = window.Get<Button>("RegisterDonationButton");

            registerButton.Click();

            var registerWindow = app.GetWindow("Register Donation Window");
            Assert.NotNull(registerWindow);

            app.Close();
        }

        [Fact]
        public void CanOpenStatisticsWindowForResearchStudent()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("MainWindow");
            var loginButton = window.Get<Button>("LoginButton");

            loginButton.Click();

            var statisticsWindow = app.GetWindow("StatisticsWindow");
            Assert.NotNull(statisticsWindow);

            app.Close();
        }

        [Fact]
        public void CanOpenChangeRoleWindow()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("MainWindow");
            var changeRoleButton = window.Get<Button>("ChangeRoleButton");

            changeRoleButton.Click();

            var roleWindow = app.GetWindow("Change Role Window");
            Assert.NotNull(roleWindow);

            app.Close();
        }

        [Fact]
        public void CanChangeUserRole()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("Change Role Window");

            var roleComboBox = window.Get<ComboBox>("RoleComboBox");
            roleComboBox.Select("SuperUser");

            window.Get<Button>("ApplyButton").Click();

            var currentRole = window.Get<Label>("CurrentRoleLabel");
            Assert.Equal("SuperUser", currentRole.Text);

            app.Close();
        }

        [Fact]
        public void CanDisplayBloodTotalsInStatisticsWindow()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("StatisticsWindow");

            var bloodTotalsLabel = window.Get<Label>("BloodTotalsLabel");
            Assert.Equal("O+: 100 units", bloodTotalsLabel.Text);  // Example check

            app.Close();
        }

        [Fact]
        public void CanDisplayDonationFrequency()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("StatisticsWindow");

            var frequencyLabel = window.Get<Label>("DonationFrequencyLabel");
            Assert.Equal("3 donations in the last 6 months", frequencyLabel.Text);

            app.Close();
        }

        [Fact]
        public void CanDisplayBloodTypeUsage()
        {
            var app = Application.Launch("\\SCE24-BioMedSW-Blood_Establishment-WPF.exe");
            var window = app.GetWindow("StatisticsWindow");

            var usageLabel = window.Get<Label>("BloodTypeUsageLabel");
            Assert.Equal("O- used in ER: 5 units", usageLabel.Text);

            app.Close();
        }
    }
}
