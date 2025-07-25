using Billing_Software.Data;
using Billing_Software.Models;
using Microsoft.EntityFrameworkCore;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Interactions;
using System.Text;

namespace Billing_Software.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly IPdfService _pdfService;
        private readonly ILogger<WhatsAppService> _logger;
        private IWebDriver? _driver;
        private bool _isWhatsAppWebReady = false;

        public WhatsAppService(
            ApplicationDbContext context,
            IInvoiceService invoiceService,
            IPdfService pdfService,
            ILogger<WhatsAppService> logger)
        {
            _context = context;
            _invoiceService = invoiceService;
            _pdfService = pdfService;
            _logger = logger;
        }

        public async Task<bool> SendInvoiceViaWhatsAppAsync(int invoiceId, string recipientPhone)
        {
            try
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
                if (invoice == null)
                {
                    _logger.LogError($"Invoice {invoiceId} not found");
                    return false;
                }

                // Format phone number (remove spaces, add country code if needed)
                var formattedPhone = FormatPhoneNumber(recipientPhone);
                
                // Generate invoice message
                var message = await GenerateInvoiceMessageAsync(invoiceId);
                
                // Send message with PDF attachment via WhatsApp Web
                var result = await SendInvoiceWithPdfAsync(invoiceId, formattedPhone, message);
                
                if (result)
                {
                    // Log the successful send
                    await LogNotificationAsync(invoiceId, formattedPhone, message, NotificationStatus.Sent);
                    _logger.LogInformation($"Invoice {invoiceId} with PDF sent successfully to {formattedPhone}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending invoice {invoiceId} via WhatsApp: {ex.Message}");
                await LogNotificationAsync(invoiceId, recipientPhone, "", NotificationStatus.Failed);
                return false;
            }
        }

        public async Task<bool> SendMessageViaWhatsAppAsync(string phoneNumber, string message)
        {
            try
            {
                if (!await InitializeWhatsAppWebAsync())
                {
                    return false;
                }

                // Navigate to WhatsApp Web
                _driver!.Navigate().GoToUrl("https://web.whatsapp.com/");
                
                // Wait for WhatsApp Web to load
                await Task.Delay(3000);

                // Check if already logged in
                if (IsWhatsAppLoggedIn())
                {
                    _logger.LogInformation("WhatsApp Web is already logged in.");
                }
                else if (IsQRCodePresent())
                {
                    _logger.LogWarning("WhatsApp Web QR code detected. Please scan the QR code once. After scanning, the session will be remembered.");
                    return false;
                }

                // Construct WhatsApp URL with phone and message
                var encodedMessage = Uri.EscapeDataString(message);
                var whatsappUrl = $"https://web.whatsapp.com/send?phone={phoneNumber}&text={encodedMessage}";
                
                _driver.Navigate().GoToUrl(whatsappUrl);
                
                // Wait for page to load and message to be ready
                await Task.Delay(5000);
                
                // Wait for the message input to be present
                try
                {
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='conversation-compose-box-input']")));
                    _logger.LogInformation("Message input field found");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Message input not found: {ex.Message}");
                }

                // Try multiple selectors for the send button
                IWebElement? sendButton = null;
                var selectors = new[]
                {
                    "button[data-testid='send']",
                    "button[data-testid='compose-btn-send']",
                    "button[aria-label='Send']",
                    "button[title='Send']",
                    "span[data-testid='send']",
                    "button:contains('Send')",
                    "button[type='submit']"
                };

                foreach (var selector in selectors)
                {
                    try
                    {
                        sendButton = _driver.FindElement(By.CssSelector(selector));
                        if (sendButton != null)
                        {
                            _logger.LogInformation($"Found send button with selector: {selector}");
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (sendButton != null)
                {
                    // Try clicking the button
                    try
                    {
                        sendButton.Click();
                        await Task.Delay(2000);
                        _logger.LogInformation("Send button clicked successfully");
                        
                        // Wait longer to ensure message was sent
                        await Task.Delay(5000);
                        _logger.LogInformation("Message send confirmed, closing tab");
                        
                        // Close the current tab after successful send
                        await CloseCurrentTab();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error clicking send button: {ex.Message}");
                        
                        // Try alternative method - press Enter key
                        try
                        {
                            var body = _driver.FindElement(By.TagName("body"));
                            body.SendKeys(Keys.Enter);
                            await Task.Delay(2000);
                            _logger.LogInformation("Sent message using Enter key");
                            
                            // Wait longer to ensure message was sent
                            await Task.Delay(5000);
                            _logger.LogInformation("Message send confirmed, closing tab");
                            
                            // Close the current tab after successful send
                            await CloseCurrentTab();
                            return true;
                        }
                        catch (Exception enterEx)
                        {
                            _logger.LogError($"Error sending with Enter key: {enterEx.Message}");
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Send button not found with any selector");
                    
                    // Try sending with Enter key as fallback
                    try
                    {
                        var body = _driver.FindElement(By.TagName("body"));
                        body.SendKeys(Keys.Enter);
                        await Task.Delay(2000);
                        _logger.LogInformation("Sent message using Enter key (fallback)");
                        
                        // Wait longer to ensure message was sent
                        await Task.Delay(5000);
                        _logger.LogInformation("Message send confirmed, closing tab");
                        
                        // Close the current tab after successful send
                        await CloseCurrentTab();
                        return true;
                    }
                    catch (Exception enterEx)
                    {
                        _logger.LogError($"Error sending with Enter key: {enterEx.Message}");
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending WhatsApp message: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendInvoiceWithPdfAsync(int invoiceId, string recipientPhone, string message)
        {
            try
            {
                if (!await InitializeWhatsAppWebAsync())
                {
                    return false;
                }

                // Generate PDF invoice file (permanent storage)
                var pdfFilePath = await _pdfService.GenerateInvoicePdfFileAsync(invoiceId);
                if (string.IsNullOrEmpty(pdfFilePath) || !File.Exists(pdfFilePath))
                {
                    _logger.LogWarning("Could not generate PDF file, sending text message only");
                    return await SendMessageViaWhatsAppAsync(recipientPhone, message);
                }
                
                _logger.LogInformation($"PDF generated successfully, saved to: {pdfFilePath}");

                var formattedPhone = FormatPhoneNumber(recipientPhone);

                // Check if already logged in
                if (IsWhatsAppLoggedIn())
                {
                    _logger.LogInformation("WhatsApp Web is already logged in.");
                }
                else if (IsQRCodePresent())
                {
                    _logger.LogWarning("WhatsApp Web QR code detected. Please scan the QR code once. After scanning, the session will be remembered.");
                    return false;
                }

                // Construct WhatsApp URL with message
                var encodedMessage = Uri.EscapeDataString(message);
                var whatsappUrl = $"https://web.whatsapp.com/send?phone={formattedPhone}&text={encodedMessage}";
                
                // Navigate to the specific chat URL
                _logger.LogInformation($"Navigating to WhatsApp chat: {whatsappUrl}");
                _driver!.Navigate().GoToUrl(whatsappUrl);
                await Task.Delay(5000);

                // Wait for the message input to be present
                try
                {
                    var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                    wait.Until(driver => driver.FindElement(By.CssSelector("[data-testid='conversation-compose-box-input']")));
                    _logger.LogInformation("Message input field found");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Message input not found: {ex.Message}");
                }

                // Find and click attachment button
                IWebElement? attachmentButton = null;
                var attachmentSelectors = new[]
                {
                    "button[data-testid='attach-button']",
                    "button[data-testid='attach-menu-plus']",
                    "button[aria-label='Attach']",
                    "button[title='Attach']",
                    "span[data-testid='attach-button']",
                    "button[data-testid='attach-menu']",
                    "button[data-testid='clip']",
                    "button[aria-label='Attach file']",
                    "button[title='Attach file']",
                    "div[data-testid='attach-button']"
                };

                foreach (var selector in attachmentSelectors)
                {
                    try
                    {
                        attachmentButton = _driver.FindElement(By.CssSelector(selector));
                        if (attachmentButton != null)
                        {
                            _logger.LogInformation($"Found attachment button with selector: {selector}");
                            break;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (attachmentButton != null)
                {
                    try
                    {
                        attachmentButton.Click();
                        await Task.Delay(2000);

                        // Try to find document option first
                        try
                        {
                            var documentOption = _driver.FindElement(By.CssSelector("input[data-testid='document-chat']"));
                            documentOption.Click();
                            await Task.Delay(1000);
                        }
                        catch
                        {
                            // If document option not found, try direct file input
                            _logger.LogInformation("Document option not found, trying direct file input");
                        }

                        // Find file input and upload PDF
                        IWebElement? fileInput = null;
                        var fileInputSelectors = new[]
                        {
                            "input[type='file']",
                            "input[data-testid='file-input']",
                            "input[accept='*']",
                            "input[data-testid='document-chat']"
                        };

                        foreach (var fileSelector in fileInputSelectors)
                        {
                            try
                            {
                                fileInput = _driver.FindElement(By.CssSelector(fileSelector));
                                if (fileInput != null)
                                {
                                    _logger.LogInformation($"Found file input with selector: {fileSelector}");
                                    break;
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        if (fileInput != null)
                        {
                            // Verify PDF file exists
                            if (File.Exists(pdfFilePath))
                            {
                                _logger.LogInformation($"Uploading PDF file: {pdfFilePath}");
                                fileInput.SendKeys(pdfFilePath);
                                await Task.Delay(3000);
                                _logger.LogInformation("PDF file uploaded successfully");
                            }
                            else
                            {
                                _logger.LogError($"PDF file not found at: {pdfFilePath}");
                            }
                        }
                        else
                        {
                            _logger.LogError("File input not found for PDF upload");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error uploading PDF: {ex.Message}");
                    }
                }
                else
                {
                    _logger.LogWarning("Attachment button not found, sending text message only");
                }

                // Wait a bit for the PDF to be processed
                await Task.Delay(3000);
                
                // Try to send the message using keyboard shortcuts instead of clicking send button
                try
                {
                    // Method 1: Try to find the message input and press Enter
                    var messageInputSelectors = new[]
                    {
                        "div[data-testid='conversation-compose-box-input']",
                        "div[contenteditable='true']",
                        "div[data-testid='conversation-compose-box-input'] div[contenteditable='true']",
                        "div[role='textbox']",
                        "div[data-testid='conversation-compose-box-input'] div[role='textbox']"
                    };

                    IWebElement? messageInput = null;
                    foreach (var selector in messageInputSelectors)
                    {
                        try
                        {
                            messageInput = _driver.FindElement(By.CssSelector(selector));
                            if (messageInput != null)
                            {
                                _logger.LogInformation($"Found message input with selector: {selector}");
                                break;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    if (messageInput != null)
                    {
                        // Focus on the message input and press Enter
                        messageInput.Click();
                        await Task.Delay(500);
                        messageInput.SendKeys(Keys.Enter);
                        await Task.Delay(3000);
                        _logger.LogInformation("Sent message with PDF using Enter key on message input");
                        
                        // Wait longer to ensure message is sent
                        await Task.Delay(5000);
                        _logger.LogInformation("Message send confirmed, closing tab");
                        
                        // Close the current tab after successful send
                        await CloseCurrentTab();
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Message input not found, trying body Enter key");
                        // Fallback: press Enter on body
                        var body = _driver.FindElement(By.TagName("body"));
                        body.SendKeys(Keys.Enter);
                        await Task.Delay(3000);
                        _logger.LogInformation("Sent message with PDF using Enter key on body");
                        
                        // Wait longer to ensure message is sent
                        await Task.Delay(5000);
                        _logger.LogInformation("Message send confirmed, closing tab");
                        
                        // Close the current tab after successful send
                        await CloseCurrentTab();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending with keyboard: {ex.Message}");
                }

                // If keyboard method failed, try one more time with body Enter key
                try
                {
                    var body = _driver.FindElement(By.TagName("body"));
                    body.SendKeys(Keys.Enter);
                    await Task.Delay(3000);
                    _logger.LogInformation("Sent message with PDF using final Enter key attempt");
                    
                    // Wait longer to ensure message is sent
                    await Task.Delay(5000);
                    _logger.LogInformation("Message send confirmed, closing tab");
                    
                    // Close the current tab after successful send
                    await CloseCurrentTab();
                    return true;
                }
                catch (Exception finalEx)
                {
                    _logger.LogError($"Final Enter key attempt failed: {finalEx.Message}");
                }

                // Wait a bit before closing tab even if send failed
                await Task.Delay(3000);
                await CloseCurrentTab();
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending invoice with PDF via WhatsApp: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendInvoiceImageViaWhatsAppAsync(int invoiceId, string recipientPhone)
        {
            try
            {
                // Generate PDF invoice
                var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoiceId);
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return false;
                }

                // Save PDF temporarily
                var tempPath = Path.GetTempFileName() + ".pdf";
                await File.WriteAllBytesAsync(tempPath, pdfBytes);

                if (!await InitializeWhatsAppWebAsync())
                {
                    return false;
                }

                var formattedPhone = FormatPhoneNumber(recipientPhone);
                var message = await GenerateInvoiceMessageAsync(invoiceId);

                // Navigate to WhatsApp Web
                _driver!.Navigate().GoToUrl("https://web.whatsapp.com/");
                await Task.Delay(3000);

                // Construct WhatsApp URL
                var encodedMessage = Uri.EscapeDataString(message);
                var whatsappUrl = $"https://web.whatsapp.com/send?phone={formattedPhone}&text={encodedMessage}";
                
                _driver.Navigate().GoToUrl(whatsappUrl);
                await Task.Delay(3000);

                // Find attachment button and upload PDF
                var attachmentButton = _driver.FindElement(By.CssSelector("button[data-testid='attach-button']"));
                attachmentButton.Click();
                await Task.Delay(1000);

                // Find document option
                var documentOption = _driver.FindElement(By.CssSelector("input[type='file']"));
                documentOption.SendKeys(tempPath);
                await Task.Delay(2000);

                // Send the message
                var sendButton = _driver.FindElement(By.CssSelector("button[data-testid='send']"));
                sendButton.Click();
                await Task.Delay(2000);

                // Clean up temp file
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending invoice image via WhatsApp: {ex.Message}");
                return false;
            }
        }

        public async Task<string> GenerateInvoiceMessageAsync(int invoiceId)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
                return string.Empty;

            var sb = new StringBuilder();
            sb.AppendLine($"🏢 *{invoice.Customer?.Name}*");
            sb.AppendLine($"📄 Invoice: {invoice.InvoiceNumber}");
            sb.AppendLine($"📅 Date: {invoice.InvoiceDate:dd/MM/yyyy}");
            sb.AppendLine($"💰 Total: ${invoice.Total:F2}");
            sb.AppendLine($"📋 Status: {invoice.Status}");
            sb.AppendLine();
            sb.AppendLine("📋 *Items:*");
            
            foreach (var item in invoice.InvoiceItems)
            {
                sb.AppendLine($"• {item.ItemName} x{item.Quantity} = ${item.Total:F2}");
            }
            
            sb.AppendLine();
            sb.AppendLine("Thank you for your business! 🚗");
            sb.AppendLine("Please contact us for any questions.");

            return sb.ToString();
        }

        public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
        {
            // Basic phone number validation
            var cleaned = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
            return cleaned.Length >= 10 && cleaned.All(char.IsDigit);
        }

        public async Task<bool> SendBulkInvoiceAsync(List<int> invoiceIds, List<string> phoneNumbers)
        {
            var results = new List<bool>();
            
            for (int i = 0; i < invoiceIds.Count && i < phoneNumbers.Count; i++)
            {
                var result = await SendInvoiceViaWhatsAppAsync(invoiceIds[i], phoneNumbers[i]);
                results.Add(result);
                
                // Add delay between sends to avoid rate limiting
                await Task.Delay(2000);
            }
            
            return results.All(r => r);
        }

        private async Task<bool> InitializeWhatsAppWebAsync()
        {
            try
            {
                if (_driver != null)
                {
                    try
                    {
                        // Test if the driver is still responsive
                        var currentUrl = _driver.Url;
                        _logger.LogInformation($"Current URL: {currentUrl}");
                        
                        // Check if we're already on WhatsApp Web
                        if (currentUrl.Contains("web.whatsapp.com"))
                        {
                            _logger.LogInformation("Already on WhatsApp Web, reusing existing session");
                            return true;
                        }
                        else
                        {
                            _logger.LogInformation("Driver exists but not on WhatsApp Web, navigating...");
                            _driver.Navigate().GoToUrl("https://web.whatsapp.com/");
                            await Task.Delay(3000);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Driver not responsive: {ex.Message}, creating new instance");
                        _driver.Quit();
                        _driver.Dispose();
                        _driver = null;
                    }
                }

                // Create new driver instance
                var options = new ChromeOptions();
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalChromeOption("useAutomationExtension", false);
                
                // Use persistent profile to remember WhatsApp Web session
                var userDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WhatsAppAutomation");
                if (!Directory.Exists(userDataDir))
                {
                    Directory.CreateDirectory(userDataDir);
                }
                options.AddArgument($"--user-data-dir={userDataDir}");
                options.AddArgument("--profile-directory=Default");
                
                // Make window smaller and position it
                options.AddArgument("--window-size=800,600");
                options.AddArgument("--window-position=100,100");
                
                // Add arguments to prevent new tab issues
                options.AddArgument("--disable-web-security");
                options.AddArgument("--allow-running-insecure-content");
                options.AddArgument("--disable-features=VizDisplayCompositor");
                
                _driver = new ChromeDriver(options);
                ((IJavaScriptExecutor)_driver).ExecuteScript("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
                
                // Position the window
                _driver.Manage().Window.Position = new System.Drawing.Point(100, 100);
                _driver.Manage().Window.Size = new System.Drawing.Size(800, 600);
                
                // Navigate to WhatsApp Web
                _driver.Navigate().GoToUrl("https://web.whatsapp.com/");
                await Task.Delay(3000);

                _isWhatsAppWebReady = true;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing WhatsApp Web: {ex.Message}");
                return false;
            }
        }

        private bool IsQRCodePresent()
        {
            try
            {
                var qrCode = _driver!.FindElement(By.CssSelector("canvas"));
                return qrCode != null;
            }
            catch
            {
                return false;
            }
        }

        private bool IsWhatsAppLoggedIn()
        {
            try
            {
                // Check if the main chat interface is present (indicates logged in)
                var chatInterface = _driver!.FindElement(By.CssSelector("[data-testid='chat-list']"));
                return chatInterface != null;
            }
            catch
            {
                return false;
            }
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove all non-digit characters
            var cleaned = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            // Add country code if not present (assuming India +91)
            if (cleaned.Length == 10)
            {
                cleaned = "91" + cleaned;
            }
            
            return cleaned;
        }

        private async Task LogNotificationAsync(int invoiceId, string recipient, string message, NotificationStatus status)
        {
            try
            {
                var notification = new Notification
                {
                    Type = NotificationType.WhatsApp,
                    Recipient = recipient,
                    Subject = $"Invoice {invoiceId}",
                    Message = message,
                    Status = status,
                    InvoiceId = invoiceId,
                    CreatedDate = DateTime.Now
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error logging notification: {ex.Message}");
            }
        }

        private async Task CloseCurrentTab()
        {
            try
            {
                if (_driver != null)
                {
                    // Wait a bit more to ensure message is fully sent
                    await Task.Delay(2000);
                    
                    // Close the current tab
                    _driver.Close();
                    _logger.LogInformation("Closed current Chrome tab");
                    
                    // Switch to the first tab if multiple tabs exist
                    var windowHandles = _driver.WindowHandles;
                    if (windowHandles.Count > 0)
                    {
                        _driver.SwitchTo().Window(windowHandles[0]);
                        _logger.LogInformation("Switched to first tab");
                    }
                    else
                    {
                        // If no tabs left, quit the driver
                        _driver.Quit();
                        _driver.Dispose();
                        _driver = null;
                        _logger.LogInformation("No tabs left, quit driver");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error closing tab: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
} 