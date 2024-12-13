using SendGrid;
using SendGrid.Helpers.Mail;

namespace BestShop.MyHelpers
{
	public class EmailSender
	{
        public static async Task SendEmail(string apiKey, string toEmail, string userName, string subject, string message)
        {
            var client = new SendGridClient(apiKey);

			var from = new EmailAddress("olegatoriumstore@gmail.com", "OleStore.com");
			var to = new EmailAddress(toEmail, userName);
			var plainTextContent = message;
			var htmlContent = "";

			var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
			var response = await client.SendEmailAsync(msg);

		}
	}
}
