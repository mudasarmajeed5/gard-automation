namespace GARD.Models
{
    public class Subscriber
    {
        public int id { get; set; }
        public int admin_id { get; set; } 
        public string name { get; set; }
        public string email { get; set; }
        public string subscribed_at { get; set; }
        public string status { get; set; }
        public string updated_at { get; set; }
    }

    public class Campaign
    {
        public int id { get; set; }
        public int admin_id { get; set; } 
        public string campaign_name { get; set; }
        public string content { get; set; }
        public string sent_at { get; set; }
    }
    public class LoginResponse
    {
        public int admin_id { get; set; }
    }
    public class SmtpSettings
    {
        public int id { get; set; }
        public int admin_id { get; set; }
        public string smtp_email { get; set; }
        public string smtp_password { get; set; }
        public string smtp_server { get; set; }
        public int smtp_port { get; set; }
        public bool smtp_ssl { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
    }
    public class EmailLog
    {
        public int id { get; set; }
        public int admin_id { get; set; }
        public int subscriber_id { get; set; }
        public string subscriber_email { get; set; }
        public string campaign_name { get; set; }
        public string status { get; set; }
        public string sent_at { get; set; }
    }
}
