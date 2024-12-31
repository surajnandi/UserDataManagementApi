namespace UserdataManagement.Models
{
    public class MailModel
    {
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string? Attachment { get; set; } // Optional
    }
}
