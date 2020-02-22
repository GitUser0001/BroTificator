using System;
using System.Net;
using System.Net.Mail;

namespace NotifierServiece
{
    internal static class Mail
    {
        static MailAddress fromAddress;
        const string fromPassword = "Battlefield2460658";
        const string subject = "Content Update";


        static Mail()
        {
            fromAddress = new MailAddress("turbo.global.notifier@gmail.com", "Big D.");
        }

        public static bool SendMail(string toEmail, string toName, string body)
        {
            var toAddress = new MailAddress(toEmail, toName);
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
            return true;
        }
    }
}
