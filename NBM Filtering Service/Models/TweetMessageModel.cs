using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBM_Filtering_Service.Models
{
    public class TweetMessageModel : MessageModel
    {
        public string TwitterID { get; set; }

        public TweetMessageModel(string messageID, string messageBody, string twitterID, string messageText = null) : base(messageID, messageBody, messageText)
        {
            TwitterID = twitterID;
        }
    }
}
