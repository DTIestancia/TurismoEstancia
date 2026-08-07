using MailKit.Net.Smtp;
using MimeKit;
using System.IO;
using System.Threading.Tasks;


namespace Mensageria.Services
{
    public class MensageServico
    {
        private string host;
        private int port;
        private string userName;
        private string password;
        private string EmailServico;

        // Get our parameterized configuration
        public MensageServico(string host, int port, string userName, string password, string EmailServico)
        {
            this.host = host;
            this.port = port;
            this.userName = userName;
            this.password = password;
            this.EmailServico = EmailServico;
        }
        // Use our configuration to send the email by using SmtpClient
        public Task SendEmailAsync(EmailFaleConoscoModel emailusuario)
        {

            var envelope = new MimeMessage();
            envelope.From.Add(new MailboxAddress(emailusuario.Nome, userName));
            envelope.To.Add(new MailboxAddress("Agência Virtual - Solicitação de Serviço", EmailServico));
            envelope.Cc.Add(new MailboxAddress("Agência Virtual - Solicitação de Serviço", emailusuario.Email));
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
                            builder.Attachments.Add(file.FileName, fileBytes);
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
                envelope.ResentCc.Add(new MailboxAddress("Agência Virtual - Solicitação de Serviço", emailusuario.Email));
                emailClient.Send(envelope);
                emailClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }

    }
}
