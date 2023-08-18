using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NBM_Filtering_Service.Models
{
    public class MessageModel
    {
        public string MessageID { get; set; }
        [JsonIgnore]
        public string MessageBody { get; set; }
        public string MessageText { get; set; }

        public MessageModel(string messageID, string messageBody, string messageText = null)
        {
            MessageID = messageID;
            MessageBody = messageBody;
            MessageText = messageText;
        }
    }
}
