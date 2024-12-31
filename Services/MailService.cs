using System.Net.Mail;
using System.Net;
using System.Net.Mime;
using iText.IO.Image;

namespace UserdataManagement.Services
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendMail(string email, string subject, string body, string? attachment = null)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            string smtpServer = emailSettings["Server"];
            int port = int.Parse(emailSettings["Port"]);
            string senderEmail = emailSettings["Email"];
            string senderPassword = emailSettings["Password"];
            string senderDisplayName = emailSettings["DisplayName"];

            try
            {
                //var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "logo.png");
                //byte[] imageBytes = File.ReadAllBytes(logoPath);
                //string base64Image = Convert.ToBase64String(imageBytes); // Convert to base64 string

                // Read the HTML template
                var htmlTemplate = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Files", "mail.html"));

                // Replace logo placeholder with base64 image string
                //htmlTemplate = htmlTemplate.Replace("logo.png", $"data:image/png;base64,{base64Image}");

                // Replace placeholders in the template
                htmlTemplate = htmlTemplate.Replace("{currentYear}", DateTime.Now.Year.ToString());
                htmlTemplate = htmlTemplate.Replace("{content}", body);

                using (var client = new SmtpClient(smtpServer, port))
                {
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderDisplayName),
                        Subject = subject,
                        IsBodyHtml = true,
                        //Body = GenerateEmailBody(body)
                        //Body = body
                        Body = htmlTemplate
                    };
                    mailMessage.To.Add(email);

                    // Add attachment if provided
                    if (!string.IsNullOrEmpty(attachment) && File.Exists(attachment))
                    {
                        var attachmentPath = new Attachment(attachment);
                        mailMessage.Attachments.Add(attachmentPath);
                    }

                    ////Embed image as CID (Content-ID)
                    //var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "logo.png");
                    //if (File.Exists(logoPath))
                    //{
                    //    var logoAttachment = new Attachment(logoPath);
                    //    logoAttachment.ContentId = "logo"; // This CID must match the src="cid:logo" in HTML
                    //    logoAttachment.ContentDisposition.Inline = true;
                    //    mailMessage.Attachments.Add(logoAttachment);
                    //}

                    client.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending email: " + ex.Message);
            }

        }

    }
}
