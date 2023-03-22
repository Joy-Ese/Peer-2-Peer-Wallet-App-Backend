using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WalletPayment.Models.DataObjects;
using WalletPayment.Models.Entites;
using WalletPayment.Services.Data;
using WalletPayment.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using WalletPayment.Models.DataObjects.Common;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
//using System.Net.Mail;
//using System.Net;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using MailKit.Net.Smtp;

namespace WalletPayment.Services.Services
{
    public class EmailService : IEmail
    {
        private readonly DataContext _context;
        private readonly LinkGenerator _linkGenerator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailCredentials? _emailCredentials;

        public EmailService(DataContext context, IConfiguration configuration, LinkGenerator linkGenerator, 
            IHttpContextAccessor httpContextAccessor, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _emailCredentials = configuration.GetSection("EmailCredentials").Get<EmailCredentials>();
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _linkGenerator = linkGenerator;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
            _logger.LogDebug(1, "Nlog injected into DashboardService");
        }

        public async Task<bool> SendEmail(EmailDto request, string emailUser)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("no-reply", _emailCredentials.EmailFrom));
                email.To.Add(new MailboxAddress("WalletUser", request.to = emailUser));

                email.Subject = "Welcome to The Globus Wallet App";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = "Thank You for registering with The Globus Wallet App. Enjoy a seamless experience! \u263A \ud83c\udf81"
                };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_emailCredentials.EmailHost, 587, false);
                    smtp.Authenticate(_emailCredentials.EmailUsername, _emailCredentials.EmailPassword);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendEmailPasswordReset(string Link, string emailUser)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("no-reply", _emailCredentials.EmailFrom));
                email.To.Add(new MailboxAddress("WalletUser", emailUser));

                email.Subject = "Link to reset your password";
                email.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = Link
                };

                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_emailCredentials.EmailHost, 587, false);
                    smtp.Authenticate(_emailCredentials.EmailUsername, _emailCredentials.EmailPassword);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<ForgetPasswordModel> ForgetPassword(ForgetPasswordDto emailReq)
        {
            ForgetPasswordModel forgetPassword = new ForgetPasswordModel();
            try
            {
                var userInDb = await _context.Users.FirstOrDefaultAsync(uE => uE.Email == emailReq.email);

                if (userInDb == null)
                {
                    forgetPassword.status = false;
                    forgetPassword.message = "User not found";
                    return forgetPassword;
                }

                userInDb.PasswordResetToken = CreatePasswordToken();
                userInDb.PasswordResetTokenExpiresAt = DateTime.Now.AddDays(1);
                await _context.SaveChangesAsync();

                var token = userInDb.PasswordResetToken;
                var email = userInDb.Email;

                //var callbackUrl = _linkGenerator.GetUriByAction("GetResetPassword", "Email", new { token, email }, 
                //    _httpContextAccessor.HttpContext.Request.Scheme, _httpContextAccessor.HttpContext.Request.Host);

                var callbackUrl = "http://127.0.0.1:5500/html/ResetPassword.html";

                await SendEmailPasswordReset(callbackUrl, email);

                if (forgetPassword.status)
                {
                    forgetPassword.status = false;
                    forgetPassword.message = "Email could not be sent";
                    return forgetPassword;
                }

                forgetPassword.status = true;
                forgetPassword.message = $"Forget password link is sent on {email}.";
                return forgetPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return forgetPassword;
            }
        }

        public async Task<ResetPasswordModel> ResetPassword(ResetPasswordDto resetPasswordReq)
        {
            ResetPasswordModel resetPass = new ResetPasswordModel();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordReq.email);

                if (user == null)
                {
                    resetPass.message = "Incorrect email";
                    return resetPass;
                }

                AuthService.CreatePasswordHash(resetPasswordReq.password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiresAt = null;

                await _context.SaveChangesAsync();

                resetPass.status = true;
                resetPass.message = "Password sucsessfully reset, you can now login!";
                return resetPass;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return resetPass;
            }
        }

        public async Task<bool> SendCreditEmail(string senderEmail, string recipient, string amount, string balance, string date, string username)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("no-reply", _emailCredentials.EmailFrom));
                email.To.Add(new MailboxAddress("WalletUser", senderEmail));

                email.Subject = $"Wallet App: NGN{amount} Credit transaction";

                string filePath = Path.GetFullPath("C:\\Users\\joyihama\\Documents\\FrontEnd Projects\\Wallet Payment App\\emailTemplates\\CreditAlert.html");
                if (!File.Exists(filePath))
                {
                    return _httpContextAccessor.HttpContext.Response.StatusCode.Equals(404);
                }

                string mailbody = string.Empty;

                StreamReader reader = new StreamReader(filePath);
                mailbody = reader.ReadToEnd();

                mailbody = mailbody.Replace("{FirstName}", recipient);
                mailbody = mailbody.Replace("{transtime}", date);
                mailbody = mailbody.Replace("{Amount}", amount);
                mailbody = mailbody.Replace("{Username}", username);
                mailbody = mailbody.Replace("{Balance}", balance);


                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = mailbody;
                email.Body = bodyBuilder.ToMessageBody();


                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_emailCredentials.EmailHost, 587, false);
                    smtp.Authenticate(_emailCredentials.EmailUsername, _emailCredentials.EmailPassword);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendDebitEmail(string recepientEmail, string sender, string amount2, string balance2, string date2, string username2)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("no-reply", _emailCredentials.EmailFrom));
                email.To.Add(new MailboxAddress("WalletUser", recepientEmail));

                email.Subject = $"Wallet App: NGN{amount2} Credit transaction";

                string filePath = Path.GetFullPath("C:\\Users\\joyihama\\Documents\\FrontEnd Projects\\Wallet Payment App\\emailTemplates\\DebitAlert.html");
                if (!File.Exists(filePath))
                {
                    return _httpContextAccessor.HttpContext.Response.StatusCode.Equals(404);
                }

                string mailbody = string.Empty;

                StreamReader reader = new StreamReader(filePath);
                mailbody = reader.ReadToEnd();

                mailbody = mailbody.Replace("{FirstName}", sender);
                mailbody = mailbody.Replace("{transtime}", date2);
                mailbody = mailbody.Replace("{Amount}", amount2);
                mailbody = mailbody.Replace("{Username}", username2);
                mailbody = mailbody.Replace("{Balance}", balance2);


                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = mailbody;
                email.Body = bodyBuilder.ToMessageBody();


                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_emailCredentials.EmailHost, 587, false);
                    smtp.Authenticate(_emailCredentials.EmailUsername, _emailCredentials.EmailPassword);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendDepositEmail(string selfEmail, string selfName, string selfAmount, string selfBalance, string date3)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("no-reply", _emailCredentials.EmailFrom));
                email.To.Add(new MailboxAddress("WalletUser", selfEmail));

                email.Subject = $"Wallet App: NGN{selfAmount} Credit transaction";

                string filePath = Path.GetFullPath("C:\\Users\\joyihama\\Documents\\FrontEnd Projects\\Wallet Payment App\\emailTemplates\\DepositAlert.html");
                if (!File.Exists(filePath))
                {
                    return _httpContextAccessor.HttpContext.Response.StatusCode.Equals(404);
                }

                string mailbody = string.Empty;

                StreamReader reader = new StreamReader(filePath);
                mailbody = reader.ReadToEnd();

                mailbody = mailbody.Replace("{FirstName}", selfName);
                mailbody = mailbody.Replace("{transtime}", date3);
                mailbody = mailbody.Replace("{Amount}", selfAmount);
                mailbody = mailbody.Replace("{Balance}", selfBalance);


                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = mailbody;
                email.Body = bodyBuilder.ToMessageBody();


                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(_emailCredentials.EmailHost, 587, false);
                    smtp.Authenticate(_emailCredentials.EmailUsername, _emailCredentials.EmailPassword);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return false;
            }
        }

        private string CreatePasswordToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }
    }
}







