namespace UserdataManagement.Services
{
    public interface IMailService
    {
        void SendMail(string email, string subject, string body, string? attachment = null);
    }
}
