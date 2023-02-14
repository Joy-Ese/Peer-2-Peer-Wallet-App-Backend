using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using WalletPayment.Models.DataObjects.Common;

namespace WalletPayment.Services.Services
{
    public class EmailService : IEmail
    {
        private readonly DataContext _context;
        
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly EmailCredentials? _emailCredentials;

        public EmailService(DataContext context, IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, ILogger<EmailService> logger)
        {
            _emailCredentials = configuration.GetSection("EmailCredentials").Get<EmailCredentials>();
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into DashboardService");
        }

        public async Task<bool> SendEmail(EmailDto request, string emailUser)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_emailCredentials?.EmailFrom));
                email.To.Add(MailboxAddress.Parse(request.to = emailUser));

                email.Subject = "Welcome to The Globus Wallet App";
                email.Body = new TextPart(TextFormat.Plain) 
                { 
                    Text = "Thank You for registering with The Globus Wallet App. Enjoy a seamless experience! \u263A \ud83c\udf81"
                };

                using var smtp = new SmtpClient();
                smtp.Connect(_emailCredentials?.EmailHost, 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(_emailCredentials?.EmailUsername, _emailCredentials?.EmailPassword);
                smtp.Send(email);
                smtp.Disconnect(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendCreditEmail(EmailDto request, string emailUser)
        {
            try
            {
                request.subject = "Credit Alert Trasaction on Wallet App";
                //request.body = UpdatePlaceHolders(GetEmailBody("Credit Alert"), request.placeHolders);

                await SendEmail(request, emailUser);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendDebitEmail(EmailDto request, string emailUser)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailFrom").Value));
                email.To.Add(MailboxAddress.Parse(request.to = emailUser));

                email.Subject = "Debit Alert Trasaction on Wallet App";
                email.Body = new TextPart(TextFormat.Plain)
                {
                    Text = "Thank You for registering with The Globus Wallet App. Enjoy a seamless experience! \u263A \ud83c\udf81"
                };

                using var smtp = new SmtpClient();
                smtp.Connect(_configuration.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
                smtp.Authenticate(_configuration.GetSection("EmailUsername").Value, _configuration.GetSection("EmailPassword").Value);
                smtp.Send(email);
                smtp.Disconnect(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        //private string GetEmailBody(string templateName)
        //{
        //    var body = File.ReadAllText(string.Format(templateName));
        //    return body;
        //}

        //private string UpdatePlaceHolders(string text, List<KeyValuePair<string, string>> keyValuePairs)
        //{
        //    if (!string.IsNullOrEmpty(text) && keyValuePairs != null)
        //    {
        //        foreach (var placeholder in keyValuePairs)
        //        {
        //            if (text.Contains(placeholder.Key))
        //            {
        //                text = text.Replace(placeholder.Key, placeholder.Value);
        //            }
        //        }
        //    }
        //    return text;
        //}

    }
}







