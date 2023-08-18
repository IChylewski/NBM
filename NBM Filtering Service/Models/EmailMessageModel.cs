using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBM_Filtering_Service.Models
{
    public class EmailMessageModel : MessageModel
    {
        public string Email { get; set; }
        public string SIR { get; set; }
        public string Type { get; set; }
        public string Subject { get; set; }
        public string SortCode { get; set; }
        public string NatureOfIncident { get; set; }

        public EmailMessageModel(string messageID, string messageBody, string email, string sir, string type, string subject, string sortCode, string natureOfIncident, string messageText = null) : base(messageID, messageBody, messageText)
        {
            Email = email;
            SIR = sir;
            Type = type;
            Subject = subject;
            SortCode = sortCode;
            NatureOfIncident = natureOfIncident;
        }

    }
}
