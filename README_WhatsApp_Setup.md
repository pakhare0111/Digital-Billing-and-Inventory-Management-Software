# WhatsApp Automation Setup Guide

## Overview
This billing software includes WhatsApp automation that allows you to send invoices directly to customers via WhatsApp Web without requiring the official WhatsApp Business API.

## Prerequisites

### 1. Chrome Browser
- Install Google Chrome browser on your computer
- The automation uses Chrome WebDriver

### 2. ChromeDriver
- Download ChromeDriver that matches your Chrome version from: https://chromedriver.chromium.org/
- Extract the chromedriver.exe file
- Place it in a folder that's in your system PATH, or in the same folder as your application

### 3. WhatsApp Account
- You need a WhatsApp account with a phone number
- The phone number will be used to send messages

## Setup Instructions

### Step 1: Install Dependencies
The application automatically installs the required Selenium packages when you build the project.

### Step 2: First Time Setup
1. Run the application
2. Navigate to any invoice details page
3. Click "Auto Send (Opens New Window)" button
4. A Chrome browser window will open automatically
5. Scan the QR code with your phone's WhatsApp app **ONCE**
6. After scanning, the session will be remembered for future use
7. Close the browser window when done
8. Next time, it will work automatically without scanning

### Step 3: Phone Number Format
Make sure customer phone numbers are in the correct format:
- India: +91XXXXXXXXXX (10 digits with country code)
- International: +[country code][phone number]
- Example: +919876543210

## How It Works

### Text Message Sending
1. Generates a formatted invoice message with:
   - Customer name
   - Invoice number and date
   - Total amount
   - List of items
   - Professional closing message

2. Opens WhatsApp Web with the pre-filled message
3. Automatically sends the message to the customer

### PDF Sending
1. Generates a PDF invoice
2. Opens WhatsApp Web
3. Attaches the PDF file
4. Sends both message and PDF to the customer

## Features

### ✅ What Works
- Send invoice text messages
- Send invoice PDFs
- Automatic phone number formatting
- Message templates with invoice details
- Error handling and logging
- Bulk sending capability

### ⚠️ Limitations
- Requires manual QR code scan on first use
- Chrome browser must be available
- Internet connection required
- WhatsApp Web must be accessible
- Rate limiting may apply

## Troubleshooting

### Common Issues

1. **ChromeDriver not found**
   - Download ChromeDriver and place it in your PATH
   - Or place it in the application folder

2. **QR Code not appearing**
   - Make sure Chrome browser is installed
   - Check internet connection
   - Try refreshing the page

3. **Message not sending**
   - Ensure WhatsApp Web is properly connected
   - Check if the phone number is valid
   - Verify the customer has a valid phone number

4. **PDF not attaching**
   - Ensure the PDF generation is working
   - Check file permissions
   - Verify the temporary file path

### Error Messages

- **"WhatsApp Web QR code detected"**: Scan the QR code with your phone
- **"Invalid phone number format"**: Update customer phone number
- **"Customer phone number not available"**: Add phone number to customer record
- **"Failed to send invoice"**: Check WhatsApp Web connection

## Security Notes

- This automation runs locally on your computer
- No data is sent to external services
- WhatsApp Web session is stored locally
- Phone numbers are only used for sending messages
- No customer data is stored by WhatsApp

## Best Practices

1. **Test First**: Always test with your own phone number first
2. **Backup**: Keep customer phone numbers backed up
3. **Monitor**: Check the notification logs for failed sends
4. **Update**: Keep Chrome and ChromeDriver updated
5. **Verify**: Confirm messages are received by customers

## Configuration

The WhatsApp service is configured in `appsettings.json`:

```json
{
  "WhatsAppSettings": {
    "ApiKey": "",
    "PhoneNumber": "",
    "TemplateId": "",
    "BaseUrl": "https://graph.facebook.com/v17.0"
  }
}
```

For local automation, these settings are not used, but you can add custom configurations if needed.

## Support

If you encounter issues:
1. Check the application logs
2. Verify Chrome and ChromeDriver versions match
3. Test with a simple phone number first
4. Ensure WhatsApp Web is working manually

This automation provides a cost-effective solution for small businesses to send invoices via WhatsApp without requiring the official Business API. 