using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicFarmStore
{
    public class SendEmailResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class EmailService
    {
        private SendGrid.SendGridClient _sendGridClient;

        public EmailService(string apiKey)
        {
            this._sendGridClient = new SendGrid.SendGridClient(apiKey);
        }

        public async Task<SendEmailResult> SendEmailAsync(string recipient, string subject, string htmlContent, string plainTextContent)
        {

            var from = new SendGrid.Helpers.Mail.EmailAddress("admin@codingtemplebikes.com", "Coding Temple Bikes");

            var to = new SendGrid.Helpers.Mail.EmailAddress(recipient);
            var message = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var mailResult = await _sendGridClient.SendEmailAsync(message);


            if ((mailResult.StatusCode == System.Net.HttpStatusCode.OK) || (mailResult.StatusCode == System.Net.HttpStatusCode.Accepted))
            {
                return new SendEmailResult
                {
                    Success = true
                };
            }
            else
            {
                return new SendEmailResult
                {
                    Success = false,
                    Message = await mailResult.Body.ReadAsStringAsync()
                };
            }
        }

    }
}
