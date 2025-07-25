using Billing_Software.Models.Configuration;

namespace Billing_Software.Models
{
    public class SettingsViewModel
    {
        public AppSettings AppSettings { get; set; } = new AppSettings();
        public EmailSettings EmailSettings { get; set; } = new EmailSettings();
        public WhatsAppSettings WhatsAppSettings { get; set; } = new WhatsAppSettings();
    }

    public class SystemInfoViewModel
    {
        public string ApplicationName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string Framework { get; set; } = string.Empty;
        public string OS { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public long WorkingSet { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Uptime => DateTime.Now - StartTime;
        public string WorkingSetFormatted => FormatBytes(WorkingSet);
        public string UptimeFormatted => $"{Uptime.Days}d {Uptime.Hours}h {Uptime.Minutes}m {Uptime.Seconds}s";

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    public class LogFileInfo
    {
        public string Name { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public string Path { get; set; } = string.Empty;
        public string SizeFormatted => FormatBytes(Size);
        public string LastModifiedFormatted => LastModified.ToString("yyyy-MM-dd HH:mm:ss");

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            return $"{number:n1} {suffixes[counter]}";
        }
    }

    public class BackupViewModel
    {
        public List<string> AvailableBackups { get; set; } = new List<string>();
        public DateTime LastBackupDate { get; set; }
        public string LastBackupSize { get; set; } = string.Empty;
        public bool IsBackupEnabled { get; set; } = true;
        public string BackupLocation { get; set; } = string.Empty;
    }

    public class RestoreViewModel
    {
        public List<string> AvailableRestorePoints { get; set; } = new List<string>();
        public DateTime LastRestoreDate { get; set; }
        public string LastRestoreStatus { get; set; } = string.Empty;
        public bool IsRestoreEnabled { get; set; } = true;
    }

    public class EmailTestViewModel
    {
        public string TestEmail { get; set; } = string.Empty;
        public bool IsEmailConfigured { get; set; }
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
    }

    public class WhatsAppTestViewModel
    {
        public string TestPhone { get; set; } = string.Empty;
        public bool IsWhatsAppConfigured { get; set; }
        public string ApiKey { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
    }
} 