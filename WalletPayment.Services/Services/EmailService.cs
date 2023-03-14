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
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;

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
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(_emailCredentials?.EmailFrom);
                mail.To.Add(new MailAddress(request.to = emailUser));

                mail.Subject = "Welcome to The Globus Wallet App";
                mail.Body = "Thank You for registering with The Globus Wallet App. Enjoy a seamless experience! \u263A \ud83c\udf81";

                SmtpClient smtp = new SmtpClient();

                smtp.Host = _emailCredentials?.EmailHost;
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential(_emailCredentials?.EmailUsername, _emailCredentials?.EmailPassword);
                smtp.EnableSsl = true;
                smtp.Send(mail);

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
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(_emailCredentials?.EmailFrom);
                mail.To.Add(new MailAddress(emailUser));

                mail.Subject = "Link to reset your password";
                mail.Body = Link;

                SmtpClient smtp = new SmtpClient();

                smtp.Host = _emailCredentials?.EmailHost;
                smtp.Port = 587;
                smtp.Credentials = new NetworkCredential(_emailCredentials?.EmailUsername, _emailCredentials?.EmailPassword);
                smtp.EnableSsl = true;
                smtp.Send(mail);

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
                    forgetPassword.message = "User not found";
                    return forgetPassword;
                }

                userInDb.PasswordResetToken = CreatePasswordToken();
                userInDb.PasswordResetTokenExpiresAt = DateTime.Now.AddDays(1);
                await _context.SaveChangesAsync();

                var token = userInDb.PasswordResetToken;
                var email = userInDb.Email;

                var callbackUrl = _linkGenerator.GetUriByAction("GetResetPassword", "Email", new { token, email }, 
                    _httpContextAccessor.HttpContext.Request.Scheme, _httpContextAccessor.HttpContext.Request.Host);

                await SendEmailPasswordReset(callbackUrl, email);

                forgetPassword.status = true;
                forgetPassword.message = $"Forget password link is sent on {email}. The link expires in 24 hours";
                return forgetPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return forgetPassword;
            }
        }

        public async Task<GetResetPasswordModel> GetResetPassword(string token, string email)
        {
            GetResetPasswordModel getResetPasswordModel = new GetResetPasswordModel();
            try
            {
                getResetPasswordModel = new GetResetPasswordModel { token = token, email = email };

                //string filePath = Path.GetFullPath("C:\\Users\\joyihama\\Documents\\FrontEnd Projects\\Wallet Payment App\\html\\Token.html");
                //if (!File.Exists(filePath))
                //{
                //    return getResetPasswordModel;
                //}

                //string getbody = string.Empty;
                //StreamReader reader = new StreamReader(filePath);
                //getbody = reader.ReadToEnd();

                //getbody = getbody.Replace("{email}", email);
                //getbody = getbody.Replace("{token}", token);

                //getbody = getResetPasswordModel.ToString();

                return getResetPasswordModel;
            }
            catch (Exception ex)
            {
                _logger.LogError($"AN ERROR OCCURRED... => {ex.Message}");
                return getResetPasswordModel;
            }
        }

        public async Task<ResetPasswordModel> ResetPassword(ResetPasswordDto resetPasswordReq)
        {
            ResetPasswordModel resetPass = new ResetPasswordModel();
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordReq.email && u.PasswordResetToken == resetPasswordReq.token);

                if (user == null || user.PasswordResetTokenExpiresAt < DateTime.Now)
                {
                    resetPass.message = "Incorrect email/token or token has expired";
                    return resetPass;
                }

                AuthService.CreatePasswordHash(resetPasswordReq.password, out byte[] passwordHash, out byte[] passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiresAt = null;

                await _context.SaveChangesAsync();

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
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(_emailCredentials?.EmailFrom);
                mail.To.Add(new MailAddress(senderEmail));

                mail.Subject = $"Wallet App: NGN{amount} Credit transaction";

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

                mail.Body = mailbody;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = _emailCredentials?.EmailHost;
                    smtp.Port = 587;
                    smtp.Credentials = new NetworkCredential(_emailCredentials?.EmailUsername, _emailCredentials?.EmailPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
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
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(_emailCredentials?.EmailFrom);
                mail.To.Add(new MailAddress(recepientEmail));

                mail.Subject = $"Wallet App: NGN{amount2} Debit transaction";

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

                mail.Body = mailbody;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = _emailCredentials?.EmailHost;
                    smtp.Port = 587;
                    smtp.Credentials = new NetworkCredential(_emailCredentials?.EmailUsername, _emailCredentials?.EmailPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
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
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress(_emailCredentials?.EmailFrom);
                mail.To.Add(new MailAddress(selfEmail));

                mail.Subject = $"Wallet App: NGN{selfAmount} Deposit transaction";

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

                mail.Body = mailbody;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.Host = _emailCredentials?.EmailHost;
                    smtp.Port = 587;
                    smtp.Credentials = new NetworkCredential(_emailCredentials?.EmailUsername, _emailCredentials?.EmailPassword);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
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







