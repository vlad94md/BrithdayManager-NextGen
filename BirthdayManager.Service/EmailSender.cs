using System;
using System.Net.Mail;

namespace BirthdayManager.Service
{
    public class EmailSender : IEmailSender
    {
        public bool SendMail(string toList, string subject, string body)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            try
            {
                MailAddress fromAddress = new MailAddress("birthdaymanager-test@gmail.com");
                message.From = fromAddress;
                message.To.Add(toList);             
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                smtpClient.Host = "smtp.gmail.com";   // We use gmail as our smtp client
                smtpClient.Port = 587;
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = true;
                smtpClient.Credentials = new System.Net.NetworkCredential("ambirthdaymanager@gmail.com", "Qwerty.123456");

                smtpClient.Send(message);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
