using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrganicFarmStore
{
    public class SendEmailResult
    {
        public SendEmailResult()
        {
            this.Errors = new HashSet<SendEmailError>();
        }

        public bool Success { get; set; }
        public string Message { get; set; }
        public ICollection<SendEmailError> Errors { get; set; }
    }

    public class SendEmailError
    {
        public string Message { get; set; }
        public string Field { get; set; }
        public string Help { get; set; }
    }

    //public class SendEmailResult
    //{
    //    public bool Success { get; set; }
    //    public string Message { get; set; }
    //}

    public class EmailService
    {
        private SendGrid.SendGridClient _sendGridClient;

        public EmailService(string apiKey)
        {
            _sendGridClient = new SendGrid.SendGridClient(apiKey);
        }

        public async Task<SendEmailResult> SendEmailAsync(string recipient, string subject, string htmlContent, string plainTextContent)
        {

            var from = new SendGrid.Helpers.Mail.EmailAddress("admin@O-Farmstore.com", "Organic-Farm Store");

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
