using MailKit.Net.Smtp;
using MimeKit;
using System.IO;
using System.Threading.Tasks;


namespace TurismoEstancia.Mail
{
    public class FaleConosco
    {
        private string host;
        private int port;
        private string userName;
        private string password;
        private string FaleConoscoEmail;
        private string EmailSuporte;

        // Get our parameterized configuration
        public FaleConosco(string host, int port, string userName, string password, string FaleConoscoEmail, string emailSuporte)
        {
            this.host = host;
            this.port = port;
            this.userName = userName;
            this.password = password;
            this.FaleConoscoEmail = FaleConoscoEmail;
            this.EmailSuporte = emailSuporte;
        }
        // Use our configuration to send the email by using SmtpClient
        public Task SendEmailAsync(EmailFaleConoscoModel emailusuario)
        {

            var envelope = new MimeMessage();
            envelope.From.Add(new MailboxAddress(emailusuario.Nome, userName));
            envelope.To.Add(new MailboxAddress("Fale Conosco", FaleConoscoEmail));
            envelope.Bcc.Add(new MailboxAddress("Fale Conosco", EmailSuporte));
            envelope.Subject = emailusuario.Motivo;
            envelope.ReplyTo.Add(MailboxAddress.Parse(emailusuario.Email));
            envelope.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = emailusuario.Mensagem };

            var builder = new BodyBuilder();

            byte[] fileBytes;

            if (emailusuario.Attachments != null)
            {

                var files = emailusuario.Attachments;
                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                file.CopyTo(ms);
                                fileBytes = ms.ToArray();
                            }
                            builder.Attachments.Add(file.FileName, fileBytes/*, MimeKit.ContentType.Parse(MediaTypeNames.Application.Pdf)*/);
                        }
                    }
                }
                builder.HtmlBody = emailusuario.Mensagem;
                envelope.Body = builder.ToMessageBody();
            }


            using (var emailClient = new SmtpClient())
            {
                emailClient.CheckCertificateRevocation = false;
                emailClient.Connect(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate(userName, password);
                emailClient.Send(envelope);
                emailClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }

    }
}
