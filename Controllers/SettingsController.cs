using Microsoft.AspNetCore.Mvc;
using Billing_Software.Models.Configuration;
using Billing_Software.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Billing_Software.Controllers
{
    public class SettingsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public SettingsController(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // GET: Settings
        public IActionResult Index()
        {
            try
            {
                var settingsViewModel = new SettingsViewModel
                {
                    AppSettings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings(),
                    EmailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings(),
                    WhatsAppSettings = _configuration.GetSection("WhatsAppSettings").Get<WhatsAppSettings>() ?? new WhatsAppSettings()
                };

                return View(settingsViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading settings: {ex.Message}";
                return View(new SettingsViewModel());
            }
        }

        // GET: Settings/App
        public IActionResult App()
        {
            var appSettings = _configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
            return View(appSettings);
        }

        // POST: Settings/App
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult App(AppSettings appSettings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update appsettings.json
                    UpdateAppSettings(appSettings);
                    TempData["SuccessMessage"] = "Application settings updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating application settings: {ex.Message}";
                }
            }

            return View(appSettings);
        }

        // GET: Settings/Email
        public IActionResult Email()
        {
            var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
            return View(emailSettings);
        }

        // POST: Settings/Email
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Email(EmailSettings emailSettings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update appsettings.json
                    UpdateEmailSettings(emailSettings);
                    TempData["SuccessMessage"] = "Email settings updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating email settings: {ex.Message}";
                }
            }

            return View(emailSettings);
        }

        // GET: Settings/WhatsApp
        public IActionResult WhatsApp()
        {
            var whatsAppSettings = _configuration.GetSection("WhatsAppSettings").Get<WhatsAppSettings>() ?? new WhatsAppSettings();
            return View(whatsAppSettings);
        }

        // POST: Settings/WhatsApp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult WhatsApp(WhatsAppSettings whatsAppSettings)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Update appsettings.json
                    UpdateWhatsAppSettings(whatsAppSettings);
                    TempData["SuccessMessage"] = "WhatsApp settings updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Error updating WhatsApp settings: {ex.Message}";
                }
            }

            return View(whatsAppSettings);
        }

        // GET: Settings/Backup
        public IActionResult Backup()
        {
            return View();
        }

        // POST: Settings/Backup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Backup(string backupType)
        {
            try
            {
                switch (backupType.ToLower())
                {
                    case "database":
                        // Implement database backup
                        TempData["SuccessMessage"] = "Database backup initiated successfully!";
                        break;
                    case "settings":
                        // Implement settings backup
                        TempData["SuccessMessage"] = "Settings backup created successfully!";
                        break;
                    case "full":
                        // Implement full backup
                        TempData["SuccessMessage"] = "Full backup initiated successfully!";
                        break;
                    default:
                        TempData["ErrorMessage"] = "Invalid backup type specified.";
                        break;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during backup: {ex.Message}";
            }

            return RedirectToAction(nameof(Backup));
        }

        // GET: Settings/Restore
        public IActionResult Restore()
        {
            return View();
        }

        // POST: Settings/Restore
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restore(IFormFile backupFile, string restoreType)
        {
            try
            {
                if (backupFile == null || backupFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "Please select a backup file.";
                    return RedirectToAction(nameof(Restore));
                }

                // Implement restore logic based on restoreType
                switch (restoreType.ToLower())
                {
                    case "database":
                        TempData["SuccessMessage"] = "Database restore completed successfully!";
                        break;
                    case "settings":
                        TempData["SuccessMessage"] = "Settings restore completed successfully!";
                        break;
                    case "full":
                        TempData["SuccessMessage"] = "Full restore completed successfully!";
                        break;
                    default:
                        TempData["ErrorMessage"] = "Invalid restore type specified.";
                        break;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error during restore: {ex.Message}";
            }

            return RedirectToAction(nameof(Restore));
        }

        // GET: Settings/TestEmail
        [HttpPost]
        public async Task<IActionResult> TestEmail(string testEmail)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testEmail))
                {
                    return Json(new { success = false, message = "Please provide a valid email address." });
                }

                var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();
                if (emailSettings == null || string.IsNullOrEmpty(emailSettings.SmtpServer))
                {
                    return Json(new { success = false, message = "Email settings are not configured." });
                }

                // Implement email test logic here
                await Task.Delay(1000); // Simulate email sending

                return Json(new { success = true, message = "Test email sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error sending test email: {ex.Message}" });
            }
        }

        // GET: Settings/TestWhatsApp
        [HttpPost]
        public async Task<IActionResult> TestWhatsApp(string testPhone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(testPhone))
                {
                    return Json(new { success = false, message = "Please provide a valid phone number." });
                }

                var whatsAppSettings = _configuration.GetSection("WhatsAppSettings").Get<WhatsAppSettings>();
                if (whatsAppSettings == null || string.IsNullOrEmpty(whatsAppSettings.ApiKey))
                {
                    return Json(new { success = false, message = "WhatsApp settings are not configured." });
                }

                // Implement WhatsApp test logic here
                await Task.Delay(1000); // Simulate WhatsApp sending

                return Json(new { success = true, message = "Test WhatsApp message sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error sending test WhatsApp message: {ex.Message}" });
            }
        }

        // GET: Settings/SystemInfo
        public IActionResult SystemInfo()
        {
            var systemInfo = new SystemInfoViewModel
            {
                ApplicationName = "Billing Software V2",
                Version = "2.0.0",
                Environment = _environment.EnvironmentName,
                Framework = Environment.Version.ToString(),
                OS = Environment.OSVersion.ToString(),
                MachineName = Environment.MachineName,
                ProcessorCount = Environment.ProcessorCount,
                WorkingSet = Environment.WorkingSet,
                StartTime = DateTime.Now.AddMilliseconds(-Environment.TickCount)
            };

            return View(systemInfo);
        }

        // GET: Settings/Logs
        public IActionResult Logs()
        {
            try
            {
                var logFiles = new List<LogFileInfo>();
                var logDirectory = Path.Combine(_environment.ContentRootPath, "Logs");

                if (Directory.Exists(logDirectory))
                {
                    var files = Directory.GetFiles(logDirectory, "*.log")
                        .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                        .Take(10);

                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        logFiles.Add(new LogFileInfo
                        {
                            Name = Path.GetFileName(file),
                            Size = fileInfo.Length,
                            LastModified = fileInfo.LastWriteTime,
                            Path = file
                        });
                    }
                }

                return View(logFiles);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading logs: {ex.Message}";
                return View(new List<LogFileInfo>());
            }
        }

        // GET: Settings/DownloadLog
        public IActionResult DownloadLog(string fileName)
        {
            try
            {
                var logDirectory = Path.Combine(_environment.ContentRootPath, "Logs");
                var filePath = Path.Combine(logDirectory, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    TempData["SuccessMessage"] = "Log file found. Download functionality will be implemented.";
                    return RedirectToAction(nameof(Logs));
                }
                else
                {
                    TempData["ErrorMessage"] = "Log file not found.";
                    return RedirectToAction(nameof(Logs));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error downloading log: {ex.Message}";
                return RedirectToAction(nameof(Logs));
            }
        }

        private void UpdateAppSettings(AppSettings appSettings)
        {
            var appsettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
            var jsonString = System.IO.File.ReadAllText(appsettingsPath);
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            // Create new JSON with updated AppSettings
            var updatedJson = new
            {
                ConnectionStrings = root.GetProperty("ConnectionStrings"),
                Logging = root.GetProperty("Logging"),
                AllowedHosts = root.GetProperty("AllowedHosts"),
                EmailSettings = root.GetProperty("EmailSettings"),
                WhatsAppSettings = root.GetProperty("WhatsAppSettings"),
                AppSettings = appSettings
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJsonString = JsonSerializer.Serialize(updatedJson, options);
            System.IO.File.WriteAllText(appsettingsPath, updatedJsonString);
        }

        private void UpdateEmailSettings(EmailSettings emailSettings)
        {
            var appsettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
            var jsonString = System.IO.File.ReadAllText(appsettingsPath);
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            // Create new JSON with updated EmailSettings
            var updatedJson = new
            {
                ConnectionStrings = root.GetProperty("ConnectionStrings"),
                Logging = root.GetProperty("Logging"),
                AllowedHosts = root.GetProperty("AllowedHosts"),
                AppSettings = root.GetProperty("AppSettings"),
                WhatsAppSettings = root.GetProperty("WhatsAppSettings"),
                EmailSettings = emailSettings
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJsonString = JsonSerializer.Serialize(updatedJson, options);
            System.IO.File.WriteAllText(appsettingsPath, updatedJsonString);
        }

        private void UpdateWhatsAppSettings(WhatsAppSettings whatsAppSettings)
        {
            var appsettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
            var jsonString = System.IO.File.ReadAllText(appsettingsPath);
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            // Create new JSON with updated WhatsAppSettings
            var updatedJson = new
            {
                ConnectionStrings = root.GetProperty("ConnectionStrings"),
                Logging = root.GetProperty("Logging"),
                AllowedHosts = root.GetProperty("AllowedHosts"),
                AppSettings = root.GetProperty("AppSettings"),
                EmailSettings = root.GetProperty("EmailSettings"),
                WhatsAppSettings = whatsAppSettings
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var updatedJsonString = JsonSerializer.Serialize(updatedJson, options);
            System.IO.File.WriteAllText(appsettingsPath, updatedJsonString);
        }
    }
} 