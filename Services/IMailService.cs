namespace UserdataManagement.Services
{
    public interface IMailService
    {
        public void SendMail(string email, string subject, string body, string? attachment = null, string? cc = null, string? bcc = null);
    }
}
