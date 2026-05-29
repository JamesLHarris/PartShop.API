namespace Site_2024.Web.Api.Models
{
    public class ContactEmailSettings
    {
        public string FromEmail { get; set; }
        public string FromDisplayName { get; set; }

        public string GeneralEmail { get; set; }
        public string PartsEmail { get; set; }
        public string OrdersEmail { get; set; }
        public string ReturnsEmail { get; set; }
        public string ShippingEmail { get; set; }
        public string SalesEmail { get; set; }
        public string SupportEmail { get; set; }

        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public bool EnableSsl { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
    }
}
