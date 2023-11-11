using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Web.Api.Service
{

	public class EmailService
	{
		private readonly string emailHost;
		private readonly int emailPort;
		private readonly string emailUsername;
		private readonly string emailPassword;
		private readonly bool useTls;
		

		public EmailService()
		{
			emailHost = "smtp.gmail.com";
			emailPort = 587;
			emailUsername = "appcurrency06@gmail.com";
			emailPassword = "xspxamhgoqyvadbf";
			useTls = true;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string body)
		{
			try
			{
				using (var smtpClient = new SmtpClient(emailHost))
				{
					smtpClient.Port = emailPort;
					smtpClient.Credentials = new NetworkCredential(emailUsername, emailPassword);
					smtpClient.EnableSsl = useTls;

					using (var message = new MailMessage(emailUsername, toEmail))
					{
						message.Subject = subject;
						message.Body = body;
						message.IsBodyHtml = true;

						await smtpClient.SendMailAsync(message);
					}
				}
			}
			catch (Exception ex)
			{
				// Handle any exceptions or log them as needed
				Console.WriteLine($"Failed to send email: {ex.Message}");
			}
		}
	}

}
