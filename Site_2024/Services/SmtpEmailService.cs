using Microsoft.Extensions.Options;
using Site_2024.Web.Api.Models;
using Site_2024.Web.Api.Requests;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Site_2024.Web.Api.Services
{
    public class SmtpEmailService : ISmtpEmailService
    {
        private readonly ContactEmailSettings _settings;

        public SmtpEmailService(IOptions<ContactEmailSettings> options)
        {
            _settings = options.Value;
        }

        public void SendContactEmail(ContactEmailRequest model, Part part, string requestOrigin)
        {
            string recipientEmail = GetRecipientEmail(model.InquiryType);
            string siteBaseUrl = ResolveSiteBaseUrl(requestOrigin);

            using MailMessage mail = new MailMessage();

            mail.From = new MailAddress(
                _settings.FromEmail,
                string.IsNullOrWhiteSpace(_settings.FromDisplayName)
                    ? "Site Contact Form"
                    : _settings.FromDisplayName
            );

            mail.To.Add(recipientEmail);
            mail.ReplyToList.Add(new MailAddress(model.Email, model.Name));

            mail.Subject = BuildSubject(model);
            mail.Body = BuildBody(model, part, siteBaseUrl);
            mail.IsBodyHtml = false;

            using SmtpClient client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);

            client.EnableSsl = _settings.EnableSsl;
            client.Credentials = new NetworkCredential(
                _settings.SmtpUsername,
                _settings.SmtpPassword
            );

            client.Send(mail);
        }

        private string GetRecipientEmail(string inquiryType)
        {
            string normalized = inquiryType?.Trim().ToLower();

            return normalized switch
            {
                "parts" => _settings.PartsEmail,
                "orders" => _settings.OrdersEmail,
                "returns" => _settings.ReturnsEmail,
                "shipping" => _settings.ShippingEmail,
                "wholesale" => _settings.SalesEmail,
                "website" => _settings.SupportEmail,
                _ => _settings.GeneralEmail
            };
        }

        private static string BuildSubject(ContactEmailRequest model)
        {
            string inquiryLabel = model.InquiryType?.Trim();

            if (string.IsNullOrWhiteSpace(inquiryLabel))
            {
                inquiryLabel = "General";
            }

            return $"Contact Form - {inquiryLabel} - {model.Subject}";
        }

        private static string BuildBody(ContactEmailRequest model, Part part, string siteBaseUrl)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("New Contact Form Submission");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Inquiry Type: {model.InquiryType}");
            sb.AppendLine($"Name: {model.Name}");
            sb.AppendLine($"Email: {model.Email}");
            sb.AppendLine($"Phone: {(string.IsNullOrWhiteSpace(model.Phone) ? "Not provided" : model.Phone)}");
            sb.AppendLine($"Subject: {model.Subject}");
            sb.AppendLine();
            sb.AppendLine("Customer Message:");
            sb.AppendLine(model.Message);

            if (part != null)
            {
                string customerPath = $"/browse/part/{part.Id}";
                string adminPath = $"/admin/part/{part.Id}";
                string customerUrl = BuildSiteUrl(siteBaseUrl, customerPath);
                string adminUrl = BuildSiteUrl(siteBaseUrl, adminPath);

                sb.AppendLine();
                sb.AppendLine("----------------------------------------");
                sb.AppendLine("Part Reference (added automatically by Site_2024)");
                sb.AppendLine($"Part Name: {part.Name}");
                sb.AppendLine($"Part ID: {part.Id}");
                sb.AppendLine($"Customer Page: {customerUrl}");
                sb.AppendLine($"Admin Page: {adminUrl}");
            }

            return sb.ToString();
        }

        private string ResolveSiteBaseUrl(string requestOrigin)
        {
            string validOrigin = NormalizeHttpOrigin(requestOrigin);

            if (!string.IsNullOrWhiteSpace(validOrigin))
            {
                return validOrigin;
            }

            return NormalizeHttpOrigin(_settings.SiteBaseUrl);
        }

        private static string NormalizeHttpOrigin(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                !Uri.TryCreate(value, UriKind.Absolute, out Uri uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return string.Empty;
            }

            return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }

        private static string BuildSiteUrl(string siteBaseUrl, string path)
        {
            return string.IsNullOrWhiteSpace(siteBaseUrl)
                ? path
                : $"{siteBaseUrl}{path}";
        }
    }
}
