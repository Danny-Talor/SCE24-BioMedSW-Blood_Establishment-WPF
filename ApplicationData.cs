using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Spreadsheet;
using SCE24_BioMedSW_Blood_Establishment_WPF;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

[Serializable]
public class ApplicationData
{
    public string DefaultAdminUsername { get; set; }
    public List<User> Users_ { get; set; }
    public List<Donation> Donations { get; set; }
    public Logs Logs { get; set; }

    // Default constructor initializes the lists
    public ApplicationData()
    {
        DefaultAdminUsername = "";
        Users_ = new List<User>();
        Donations = new List<Donation>();
        Logs = new Logs();
    }

    public static void SaveApplicationData(ApplicationData data)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ApplicationData));

        using (FileStream fileStream = new FileStream(Util.GetDataFilePath(Util.DataType.APPDATA), FileMode.Create))
        {
            serializer.Serialize(fileStream, data);
        }
    }

    public static ApplicationData LoadApplicationData()
    {
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ApplicationData));
            using (FileStream fileStream = new FileStream(Util.GetDataFilePath(Util.DataType.APPDATA), FileMode.Open))
            {
                return (ApplicationData)serializer.Deserialize(fileStream);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new ApplicationData();
        }
    }
}

[Serializable]
public class Logs
{
    public List<DonationLog> Donations { get; set; }
    public List<BloodTransferLog> BloodTransfers { get; set; }
    public List<MCILog> MCIs { get; set; }

    public List<ExportLog> Exports { get; set; }

    // Default constructor initializes the lists
    public Logs()
    {
        Donations = new List<DonationLog>();
        BloodTransfers = new List<BloodTransferLog>();
        MCIs = new List<MCILog>();
        Exports = new List<ExportLog>();
    }
}

[Serializable]
public class DonationLog
{
    public string FullName { get; set; }
    public string IdentificationNumber { get; set; }
    public string BloodType { get; set; }
    public DateTime DonationDate { get; set; }
    public string RegisteredBy { get; set; }
}

[Serializable]
public class BloodTransferLog
{
    public string Donor { get; set; }
    public string ID { get; set; }
    public string BloodType { get; set; }
    public string RequestedBloodType { get; set; }
    public int RequestedAmount { get; set; }
    public int TransferredAmount { get; set; } // New property
    public string RequestedDepartment { get; set; }
    public DateTime Timestamp { get; set; }
    public string User { get; set; }
}

[Serializable]
public class MCILog
{
    public int AmountSent { get; set; }
    public DateTime Timestamp { get; set; }
    public string User { get; set; }
}

[Serializable]
public class ExportLog
{ 
    public DateTime Timestamp { get; set; }
    public string User { get; set; }
}

[Serializable]
public class User
{
    public string Username { get; set; }
    public string Password { get; set; }
    public int Role { get; set; }
    public User()
    {
        this.Username = "none";
        this.Password = "none";
        this.Role = 0;
    }
    public User(string username, string password, int role)
    {
        this.Username = username;
        this.Password = password;
        this.Role = role;
    }
}