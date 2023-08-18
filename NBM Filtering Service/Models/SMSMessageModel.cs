using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBM_Filtering_Service.Models
{
    public class SMSMessageModel : MessageModel
    {
        public string PhoneNumber { get; set; }

        public SMSMessageModel(string messageID, string messageBody, string phoneNumber, string messageText = null) : base(messageID, messageBody, messageText)
        {
            PhoneNumber = phoneNumber;
        }
    }
}
