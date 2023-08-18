using NBM_Filtering_Service.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBM_Filtering_Service.Views
{
    internal class ReaderView
    {
        private MainWindow mainWindow;
        public List<SMSMessageModel> smsMessages;
        public List<EmailMessageModel> emailMessages;
        public List<TweetMessageModel> tweetMessages;

        public ReaderView(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        // Loads messages from the json files and populates the Lists
        public void LoadAllMessages()
        {
            string smsFilePath = "../../../History/SMSMessages.json";
            string smsExistingJsonContent = File.ReadAllText(smsFilePath);
            smsMessages = JsonConvert.DeserializeObject<List<SMSMessageModel>>(smsExistingJsonContent);

            string emailFilePath = "../../../History/EmailMessages.json";
            string emailExistingJsonContent = File.ReadAllText(emailFilePath);
            emailMessages = JsonConvert.DeserializeObject<List<EmailMessageModel>>(emailExistingJsonContent);

            string tweetFilePath = "../../../History/TweetMessages.json";
            string tweetExistingJsonContent = File.ReadAllText(tweetFilePath);
            tweetMessages = JsonConvert.DeserializeObject<List<TweetMessageModel>>(tweetExistingJsonContent);
        }
        // Populates fields of the reader depending on message type selected
        public void InitializeReader(string type)
        {
            switch(type)
            {
                case "Sms":
                    mainWindow.smsIDReaderField.Text = smsMessages[0].MessageID.ToString();
                    mainWindow.smsPhoneReaderField.Text = smsMessages[0].PhoneNumber.ToString();
                    mainWindow.smsTextReaderField.Text = smsMessages[0].MessageText.ToString();
                    break;
                case "Email":
                    mainWindow.emailIDReaderField.Text = emailMessages[0].MessageID.ToString();
                    mainWindow.emailSenderReaderField.Text = emailMessages[0].Email.ToString();
                    mainWindow.emailTypeReaderField.Text = emailMessages[0].Type.ToString();
                    mainWindow.emailTextReaderField.Text = emailMessages[0].MessageText.ToString();
                    mainWindow.emailSubjectReaderField.Text = emailMessages[0].Subject.ToString();
                    if(emailMessages[0].Type.ToString() == "SIR")
                    {
                        mainWindow.emailSortCodeReaderField.Text = emailMessages[0].SortCode.ToString();
                        mainWindow.emailNoTReaderField.Text = emailMessages[0].NatureOfIncident.ToString();
                    }
                    break;
                case "Tweet":
                    mainWindow.tweetIDReaderField.Text = tweetMessages[0].MessageID.ToString();
                    mainWindow.tweetSenderIDReaderField.Text = tweetMessages[0].TwitterID.ToString();
                    mainWindow.tweetTextReaderField.Text = tweetMessages[0].MessageText.ToString();
                    break;
            }
        }
        // Updates the readers fields with values of new message
        public void NextMessage(string type, int counter)
        {
            try
            {
                switch (type)
                {
                    case "Sms":
                        mainWindow.smsIDReaderField.Text = smsMessages[counter].MessageID.ToString();
                        mainWindow.smsPhoneReaderField.Text = smsMessages[counter].PhoneNumber.ToString();
                        mainWindow.smsTextReaderField.Text = smsMessages[counter].MessageText.ToString();
                        break;
                    case "Email":
                        mainWindow.emailIDReaderField.Text = emailMessages[counter].MessageID.ToString();
                        mainWindow.emailSenderReaderField.Text = emailMessages[counter].Email.ToString();
                        mainWindow.emailTypeReaderField.Text = emailMessages[counter].Type.ToString();
                        mainWindow.emailTextReaderField.Text = emailMessages[counter].MessageText.ToString();
                        mainWindow.emailSubjectReaderField.Text = emailMessages[counter].Subject.ToString();
                        if (emailMessages[0].Type.ToString() == "SIR")
                        {
                            mainWindow.emailSortCodeReaderField.Text = emailMessages[counter].SortCode.ToString();
                            mainWindow.emailNoTReaderField.Text = emailMessages[counter].NatureOfIncident.ToString();
                        }
                        break;
                    case "Tweet":
                        mainWindow.tweetIDReaderField.Text = tweetMessages[counter].MessageID.ToString();
                        mainWindow.tweetSenderIDReaderField.Text = tweetMessages[counter].TwitterID.ToString();
                        mainWindow.tweetTextReaderField.Text = tweetMessages[counter].MessageText.ToString();
                        break;
                }
            }
            catch 
            {
            }
        }
        // Updates the readers fields with values of previous message
        public void PreviousMessage(string type, int counter)
        {
            try
            {
                switch (type)
                {
                    case "Sms":
                        mainWindow.smsIDReaderField.Text = smsMessages[counter].MessageID.ToString();
                        mainWindow.smsPhoneReaderField.Text = smsMessages[counter].PhoneNumber.ToString();
                        mainWindow.smsTextReaderField.Text = smsMessages[counter].MessageText.ToString();
                        break;
                    case "Email":
                        mainWindow.emailIDReaderField.Text = emailMessages[counter].MessageID.ToString();
                        mainWindow.emailSenderReaderField.Text = emailMessages[counter].Email.ToString();
                        mainWindow.emailTypeReaderField.Text = emailMessages[counter].Type.ToString();
                        mainWindow.emailTextReaderField.Text = emailMessages[counter].MessageText.ToString();
                        mainWindow.emailSubjectReaderField.Text = emailMessages[counter].Subject.ToString();
                        if (emailMessages[0].Type.ToString() == "SIR")
                        {
                            mainWindow.emailSortCodeReaderField.Text = emailMessages[counter].SortCode.ToString();
                            mainWindow.emailNoTReaderField.Text = emailMessages[counter].NatureOfIncident.ToString();
                        }
                        break;
                    case "Tweet":
                        mainWindow.tweetIDReaderField.Text = tweetMessages[counter].MessageID.ToString();
                        mainWindow.tweetSenderIDReaderField.Text = tweetMessages[counter].TwitterID.ToString();
                        mainWindow.tweetTextReaderField.Text = tweetMessages[counter].MessageText.ToString();
                        break;
                }
            }
            catch
            {
            }
        }
    }
}
