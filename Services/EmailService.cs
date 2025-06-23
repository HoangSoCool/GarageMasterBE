using GarageMasterBE.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GarageMasterBE.Services
{
    public class EmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value ?? throw new ArgumentNullException(nameof(smtpSettings));

            if (string.IsNullOrWhiteSpace(_smtpSettings.FromEmail))
                throw new ArgumentException("SMTP FromEmail cannot be null or empty.", nameof(_smtpSettings.FromEmail));
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Email address cannot be null or empty.", nameof(toEmail));

            try
            {
                using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.FromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // TODO: Log exception here if you have a logging service
                // _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw new InvalidOperationException($"Gửi email đến {toEmail} thất bại.", ex);
            }
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Confirmation code cannot be null or empty.", nameof(code));

            string subject = "GarageMaster - Xác thực tài khoản của bạn";
            string body = $@"
    <div style='font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #e5e7eb;border-radius:12px;padding:32px 24px;background:#f9fafb;'>
        <div style='text-align:center;margin-bottom:24px;'>
            <img src='https://cdn-icons-png.flaticon.com/512/3208/3208720.png' alt='GarageMaster' width='64' style='margin-bottom:8px;'/>
            <h2 style='color:#2563eb;margin:0 0 8px 0;'>Xác nhận đăng ký GarageMaster</h2>
        </div>
        <p style='font-size:16px;color:#222;text-align:center;'>Chào bạn,</p>
        <p style='font-size:16px;color:#222;margin-bottom:24px;text-align:center;'>
            Cảm ơn bạn đã đăng ký tài khoản GarageMaster.<br/>
            Đây là <b>mã xác thực</b> để kích hoạt tài khoản của bạn:
        </p>
        <div style='text-align:center;margin:32px 0;'>
            <span style='display:inline-block;font-size:2.5rem;font-weight:bold;letter-spacing:0.5rem;
                color:#d97706;background:#fef08a;padding:18px 36px;border-radius:14px;
                border:2px solid #f59e42;box-shadow:0 2px 12px #f59e4222;'>
                {code}
            </span>
        </div>
        <p style='font-size:15px;color:#444;margin-bottom:16px;text-align:center;'>
            <b>Lưu ý:</b> Mã này chỉ có hiệu lực trong <span style='color:#dc2626;font-weight:bold;'>1 phút</span>.
        </p>
        <p style='font-size:15px;color:#666;margin-bottom:0;text-align:center;'>
            Nếu bạn không thực hiện đăng ký, vui lòng bỏ qua email này.<br/>
            <br/>
            Trân trọng,<br/>
            <span style='color:#2563eb;font-weight:bold;'>GarageMaster Team</span>
        </p>
    </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendNewPasswordEmailAsync(string toEmail, string newPassword)
        {
            string subject = "GarageMaster - Mật khẩu mới của bạn";
            string body = $@"
    <div style='font-family:Segoe UI,Arial,sans-serif;max-width:480px;margin:auto;border:1px solid #e5e7eb;border-radius:12px;padding:32px 24px;background:#f9fafb;'>
        <div style='text-align:center;margin-bottom:24px;'>
            <img src='https://cdn-icons-png.flaticon.com/512/3208/3208720.png' alt='GarageMaster' width='64' style='margin-bottom:8px;'/>
            <h2 style='color:#2563eb;margin:0 0 8px 0;'>Mật khẩu mới cho tài khoản GarageMaster</h2>
        </div>
        <p style='font-size:16px;color:#222;text-align:center;'>Xin chào,</p>
        <p style='font-size:16px;color:#222;margin-bottom:24px;text-align:center;'>
            Mật khẩu mới của bạn là:
        </p>
        <div style='text-align:center;margin:32px 0;'>
            <span style='display:inline-block;font-size:2.2rem;font-weight:bold;letter-spacing:0.4rem;
                color:#fff;background:#2563eb;padding:16px 32px;border-radius:14px;
                border:2px solid #2563eb;box-shadow:0 2px 12px #2563eb22;'>
                {newPassword}
            </span>
        </div>
        <p style='font-size:15px;color:#444;margin-bottom:16px;text-align:center;'>
            Hãy đăng nhập và đổi lại mật khẩu để bảo mật tài khoản.
        </p>
        <p style='font-size:15px;color:#666;margin-bottom:0;text-align:center;'>
            Nếu bạn không yêu cầu, vui lòng bỏ qua email này.<br/>
            <br/>
            Trân trọng,<br/>
            <span style='color:#2563eb;font-weight:bold;'>GarageMaster Team</span>
        </p>
    </div>";
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
