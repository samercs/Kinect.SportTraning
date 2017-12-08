using System.Net;
using System.Net.Mail;

namespace LightBuzz.Vituvius.Samples.WPF
{
    public class EmailService
    {
        public static void SendEmail(int count, string traingType)
        {
            if (count <= 0)
            {
                return;
            }
            var fromAddress = new MailAddress("traning.application@gmail.com", "Traning Application");
            var toAddress = new MailAddress("traning.application@gmail.com", "Traning Application");
            const string fromPassword = "traing!@#";
            const string subject = "Patient using report";
            string body = "<h3>Hi System Admin,</h3> <br/><p>We send this email as <b>Traning Application</b> using report.</p>";
            body += "<p>Patient do training of type <b>" + traingType + "</b>, <b>" + count + "</b> times</p>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }
}