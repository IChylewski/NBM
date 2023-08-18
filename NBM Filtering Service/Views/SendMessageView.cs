using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Win32;
using NBM_Filtering_Service.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace NBM_Filtering_Service.Views
{
    public class SendMessageView
    {
        private MainWindow mainWindow;
        private Dictionary<string, string> abbreviationDictionary;
        private List<string> possibleNOT;

        public SendMessageView(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            abbreviationDictionary = LoadAbbreviations();
            possibleNOT = new List<string>() // Contains possible Nature of Incidents types
            {
                "[Theft]","[Staff Attack]","[Raid]","[Customer Attack]", "[Staff Abuse]", "[Bomb Threat]", "[Terrorism]", "[Suspicious Incident]", "[Intelligence]", "[Cash Loss]"
            };
        }
        // First validates and if successfull sanity and saves message
        public bool VSCMessage(MessageModel message)
        {
            // Validate
            bool validate = ValidateMessage(message);

            if(validate == false)
            {
                return false;
            }

            // Sanity and Categorize and Save
            SanityMessage(message);

            return true;
        }
        // This method validates message, validation method is chosen depending on message type
        // The method uses reggex pattern to match specific format of the parameters
        public bool ValidateMessage(MessageModel message)
        {
            bool validated = true;
            mainWindow.MessageIDErrorText.Visibility = Visibility.Collapsed;
            mainWindow.MessageErrorText.Visibility = Visibility.Collapsed;

            if (message.MessageID.Length == 0) 
            {
                mainWindow.MessageIDErrorText.Content = "ID must be 10 characters long";
                mainWindow.MessageIDErrorText.Visibility = Visibility.Visible;
                validated = false;
                return validated;
            }
            switch (message.MessageID[0]) //Checks the ID first letter to determine what type of message it is
            {
                //Phone-Number: [+447746252576] This is test SMS message ROTFL B4N test test // sample SMS message for debugging
                case 'S':
                    if(message.MessageID.Length < 10 || message.MessageID.Length > 10)
                    {
                        //Error display
                        mainWindow.MessageIDErrorText.Content = "ID must be 10 characters long";
                        mainWindow.MessageIDErrorText.Visibility = Visibility.Visible;
                        validated = false;
                    }

                    string phoneNumberPattern = @"(?<=Phone-Number:\s)\[([^\]]{7,25})\]";
                    Match match = Regex.Match(message.MessageBody, phoneNumberPattern);

                    if (!match.Success)
                    {
                        //Error display
                        mainWindow.MessageErrorText.Content = "Please provide phone number in Phone-Number: [xyz] format";
                        mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                        validated = false;
                        return validated;
                    }

                    // checking text length
                    int followingTextStartIndexSMS = match.Index + match.Length;
                    int remainingLengthSMS = message.MessageBody.Substring(followingTextStartIndexSMS).Length;
                    if (remainingLengthSMS > 140 || remainingLengthSMS < 20)
                    {
                        //Error display
                        mainWindow.MessageErrorText.Content = "Message text must be between 20 and 140 characters long";
                        mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                        validated = false;
                    }
                    return validated;

                case 'E':
                    /*semirek2301@gmail.com SIR 23/09/1997 Sort-Code: [88-88-88] Nature-of-Incident:
                    [Bomb Threat] Subject: [This is test email] This is test email which contains URLs like www.google.com*/

                    if (message.MessageID.Length < 10 || message.MessageID.Length > 10)
                    {
                        //Error display
                        mainWindow.MessageIDErrorText.Content = "ID must be 10 characters long";
                        mainWindow.MessageIDErrorText.Visibility = Visibility.Visible;
                        validated = false;
                    }

                    string emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
                    string subjectPattern = @"(?<=Subject:\s)\[([^\]]{1,20})\]";
                    string sirPattern = @"\bSIR\s\d{2}/\d{2}/\d{4}\b";
                    string sortcodePattern = @"(?<=Sort-Code:\s)\[([^\]]{8})\]";
                    string natureOfIncidentPattern = @"(?<=Nature-of-Incident:\s)\[([^\]]{1,20})\]";

                    Match emailMatch = Regex.Match(message.MessageBody, emailPattern, RegexOptions.IgnoreCase);
                    Match subjectMatch = Regex.Match(message.MessageBody, subjectPattern, RegexOptions.IgnoreCase);
                    Match sirMatch = Regex.Match(message.MessageBody, sirPattern, RegexOptions.IgnoreCase);
                    Match sortcodeMatch = Regex.Match(message.MessageBody, sortcodePattern, RegexOptions.IgnoreCase);
                    Match natureOfIncidentMatch = Regex.Match(message.MessageBody, natureOfIncidentPattern, RegexOptions.IgnoreCase);

                    if (!emailMatch.Success)
                    {
                        //Error display
                        validated = false;
                        mainWindow.MessageErrorText.Content = "Please provide email: xyz@email.com";
                        mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                        return validated;
                    }
                    if(!subjectMatch.Success)
                    {
                        //Error display
                        validated = false;
                        mainWindow.MessageErrorText.Content = "Please provide subject max 20 characters: Subject: [xyz]";
                        mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                        return validated;
                    }
                    if(sirMatch.Success)
                    {
                        if(!natureOfIncidentMatch.Success)
                        {
                            //Error display
                            validated = false;
                            mainWindow.MessageErrorText.Content = "Please provide Nature of Incident in format Nature-of-Incident: [xyz]";
                            mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                            return validated;
                        }
                        if (!sortcodeMatch.Success)
                        {
                            //Error display
                            validated = false;
                            mainWindow.MessageErrorText.Content = "Please provide sort code in format Sort-Code: [xyz]";
                            mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                            return validated;
                        }
                        if (!possibleNOT.Contains(natureOfIncidentMatch.Value))
                        {
                            //Error display
                            validated = false;
                            mainWindow.MessageErrorText.Content = "Please provide correct Nature of Incident example: Theft ";
                            mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                            return validated;
                        }
                    }
                    // The last parameter is always Subject: [xyz] and the remaining text length is calculated from the end of it
                    int followingTextStartIndex = subjectMatch.Index + subjectMatch.Length;
                    int remainingLength = message.MessageBody.Substring(followingTextStartIndex).Length;
                    if (remainingLength > 1028 || remainingLength < 20)
                    {
                        //Error display
                        mainWindow.MessageErrorText.Content = "Message text must be between 20 and 1028 characters long";
                        mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                        validated = false;
                    }
                    return validated;
                case 'T':
                    //@TestSender This is tweet it contains @mentions #hashtags and different abbreviations like AFK // sample tweet message for debugging
                    if (message.MessageID.Length < 10 || message.MessageID.Length > 10)
                    {
                        //Error display
                        mainWindow.MessageIDErrorText.Content = "ID must be 10 characters long";
                        mainWindow.MessageIDErrorText.Visibility = Visibility.Visible;
                        validated = false;
                    }

                    string twitterIDPattern = @"@\w{1,16}";
                    Match twitterIDMatch = Regex.Match(message.MessageBody, twitterIDPattern);

                    if (!twitterIDMatch.Success)
                    {
                        //Error display
                        mainWindow.MessageErrorText.Content = "Twitter ID is missing";
                        mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                        validated = false;
                        return validated;
                    }
                    // checking remaining text length
                    int followingTextStartIndexTweet = twitterIDMatch.Index + twitterIDMatch.Length;
                    int remainingLengthTweet = message.MessageBody.Substring(followingTextStartIndexTweet).Length;
                    if (remainingLengthTweet > 140 || remainingLengthTweet < 5)
                    {
                        //Error display
                        mainWindow.MessageErrorText.Content = "Tweet must be between 5 and 140 characters long";
                        mainWindow.MessageErrorText.Visibility = Visibility.Visible;
                        validated = false;
                    }

                    return validated;
                default:
                    if (message.MessageID[0] != 'S' || message.MessageID[0] != 'E' || message.MessageID[0] != 'T')
                    {
                        //Error display
                        mainWindow.MessageIDErrorText.Content = "Incorrect Message ID";
                        mainWindow.MessageIDErrorText.Visibility = Visibility.Visible;
                        validated = false;
                    }
                    return validated;
            }
        }
        public void SanityMessage(MessageModel message)
        {
            switch (message.MessageID[0])
            {
                case 'S':
                    SaveMessage(ProcessSMSMessage(message));
                    break;
                case 'E':
                    SaveMessage(ProcessEmailMessage(message));
                    break;
                case 'T':
                    SaveMessage(ProcessTweetMessage(message));
                    break;
                default:
                    break;
            }
        }
        // Loads abbreviations from the excel file and returns it as dictionary
        public Dictionary<string, string> LoadAbbreviations()
        {
            Dictionary<string, string> abbreviationDictionary = new Dictionary<string, string>();
            string filePath = "../../../Resources/Abbreviations.xlsx";
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        string abbreviation = worksheet.Cells[row, 1].Text;
                        string meaning = worksheet.Cells[row, 2].Text;

                        abbreviationDictionary[abbreviation] = meaning;
                    }
                }
            }
            catch
            {
                mainWindow.CloseFile();
                return LoadAbbreviations();
            }
            return abbreviationDictionary;
        }
        // Removes words from the string and removes white characters it is used to process message text
        public string RemoveWordAndSpaces(string input, string[] wordsToRemove)
        {
            string pattern = string.Join("|", wordsToRemove.Select(Regex.Escape));
            string result = Regex.Replace(input, pattern, string.Empty);
            result = Regex.Replace(result, @"\s+", " ").Trim();
            return result;
        }
        // Takes message as input processes it and returns it as sms message model
        public SMSMessageModel ProcessSMSMessage(MessageModel message)
        {
            message.MessageText = message.MessageBody;
            // Matches all abbreviations from the text
            MatchCollection matches = Regex.Matches(message.MessageBody, @"\b[A-Z0-9!@#$%^&*()-_=+{}\[\]:;""'<>,.?/\\|]+\b");
            HashSet<string> uniqueMatches = new HashSet<string>();

            foreach (Match match in matches)
            {
                string abbreviation = match.Value.ToUpper();
                uniqueMatches.Add(abbreviation);
            }
            // Adds meaning to the abbreviations in the text
            foreach (string abbreviation in uniqueMatches)
            {
                if (abbreviationDictionary.TryGetValue(abbreviation, out string? meaning))
                {
                    string pattern = $@"\b{Regex.Escape(abbreviation)}\b";
                    message.MessageText = Regex.Replace(message.MessageText, pattern, $"{abbreviation} <{meaning}>", RegexOptions.IgnoreCase);
                }
            }

            string phoneNumberPattern = @"(?<=Phone-Number:\s)\[([^\]]{7,25})\]";
            Match phoneNumberMatch = Regex.Match(message.MessageBody, phoneNumberPattern);

            // Removes message parameters from the text
            string[] wordsToRemove = { "Phone-Number: ", phoneNumberMatch.Value};
            message.MessageText = RemoveWordAndSpaces(message.MessageText, wordsToRemove);

            SMSMessageModel smsMessage = new SMSMessageModel(message.MessageID, message.MessageBody, phoneNumberMatch.Groups[1].Value, message.MessageText);

            return smsMessage;
        }
        // Takes message as input processes it and returns it as email message model
        public EmailMessageModel ProcessEmailMessage(MessageModel message)
        {
            message.MessageText = message.MessageBody;

            string urlPattern = @"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})";

            // Matches all URLs in the text
            MatchCollection matches = Regex.Matches(message.MessageText, urlPattern);
            HashSet<string> uniqueMatches = new HashSet<string>();

            foreach (Match match in matches)
            {
                string link = match.Value;
                uniqueMatches.Add(link);
            }
            // Replaces URLs with placeholder
            foreach (string link in uniqueMatches)
            {
                string pattern = $@"\b{Regex.Escape(link)}\b";
                message.MessageText = Regex.Replace(message.MessageText, pattern, $"<URL Quarantined>");
                AddToQuarantine(link);
            }

            string emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            string sirPattern = @"\bSIR\s\d{2}/\d{2}/\d{4}\b";
            string subjectPattern = @"(?<=Subject:\s)\[([^\]]{1,20})\]";
            string sortcodePattern = @"(?<=Sort-Code:\s)\[([^\]]{8})\]";
            string natureOfIncidentPattern = @"(?<=Nature-of-Incident:\s)\[([^\]]{1,20})\]";

            Match emailMatch = Regex.Match(message.MessageBody, emailPattern, RegexOptions.IgnoreCase);
            Match sirMatch = Regex.Match(message.MessageBody, sirPattern, RegexOptions.IgnoreCase);
            string type = "Normal";

            Match subjectMatch = Regex.Match(message.MessageBody, subjectPattern, RegexOptions.IgnoreCase);
            Match sortcodeMatch = Regex.Match(message.MessageBody, sortcodePattern, RegexOptions.IgnoreCase);
            Match notMatch = Regex.Match(message.MessageBody, natureOfIncidentPattern, RegexOptions.IgnoreCase);
            string[] wordsToRemove = { emailMatch.Value, "Subject: ", subjectMatch.Value };

            //if SIR found then type of message is SIR
            if (sirMatch.Success)
            {
                type = "SIR";
                AddToSIRList(sirMatch.Value, notMatch.Value);
                wordsToRemove = new string[] { emailMatch.Value, sirMatch.Value, "Subject: ", subjectMatch.Value, "Sort-Code: ", sortcodeMatch.Value, "Nature-of-Incident: ", notMatch.Value };
            }
            
            message.MessageText = RemoveWordAndSpaces(message.MessageText, wordsToRemove);

            EmailMessageModel emailMessage = new EmailMessageModel(message.MessageID, message.MessageBody, emailMatch.Value, sirMatch.Value, type, subjectMatch.Groups[1].Value, sortcodeMatch.Groups[1].Value, notMatch.Groups[1].Value, message.MessageText);

            return emailMessage;
        }
        // Takes message as input processes it and return as Tweet message
        public TweetMessageModel ProcessTweetMessage(MessageModel message)
        {
            message.MessageText = message.MessageBody;

            MatchCollection matches = Regex.Matches(message.MessageBody, @"\b[A-Z0-9!@#$%^&*()-_=+{}\[\]:;""'<>,.?/\\|]+\b");
            HashSet<string> uniqueMatches = new HashSet<string>();

            // Matches all abbreviations
            foreach (Match match in matches)
            {
                string abbreviation = match.Value.ToUpper();
                uniqueMatches.Add(abbreviation);
            }
            // Adds definition to abbreviation
            foreach (string abbreviation in uniqueMatches)
            {
                if (abbreviationDictionary.TryGetValue(abbreviation, out string? meaning))
                {
                    string pattern = $@"\b{Regex.Escape(abbreviation)}\b";
                    message.MessageText = Regex.Replace(message.MessageText, pattern, $"{abbreviation} <{meaning}>", RegexOptions.IgnoreCase);
                }
            }
            // Matches twitterID
            string twitterIDPattern = @"@\w{1,15}";
            Match twitterIDMatch = Regex.Match(message.MessageBody, twitterIDPattern);
            AddToMentionList(message);
            AddToTrendingList(message);

            string[] wordsToRemove = {twitterIDMatch.Value};
            message.MessageText = RemoveWordAndSpaces(message.MessageText, wordsToRemove);

            TweetMessageModel tweetMessage = new TweetMessageModel(message.MessageID, message.MessageBody, twitterIDMatch.Value, message.MessageText);

            return tweetMessage;
        }
        // Adds links found in the text to quarantine list in excel file
        public void AddToQuarantine(string url)
        {
            // check if unique
            string filePath = "../../../History/QuarantineList.xlsx";
            try
            {
                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    ExcelWorksheet quarantineList = package.Workbook.Worksheets[0];
                    int rowCount = quarantineList.Dimension.Rows;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        if (quarantineList.Cells[row, 1].Text == url)
                        {
                            return;
                        }
                    }

                    quarantineList.Cells[rowCount + 1, 1].Value = url;
                    package.Save();
                }
            }
            catch
            {
                mainWindow.CloseFile();
                AddToQuarantine(url);
            }

        }
        // Adds sir found in the text to sir list in excel file
        public void AddToSIRList(string sir, string natureOfIncident)
        {
            string filePath = "../../../History/SIRList.xlsx";

            try
            {
                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    ExcelWorksheet sirList = package.Workbook.Worksheets[0];
                    int rowCount = sirList.Dimension.Rows;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        if (sirList.Cells[row, 1].Text == sir)
                        {
                            return;
                        }
                    }

                    sirList.Cells[rowCount + 1, 1].Value = sir;
                    sirList.Cells[rowCount + 1, 2].Value = natureOfIncident;
                    package.Save();
                }
            }
            catch
            {
                mainWindow.CloseFile();
                AddToSIRList(sir, natureOfIncident);
            }

        }
        // Adds mentions found in the text to mention list in excel file
        public void AddToMentionList(MessageModel message)
        {
            string filePath = "../../../History/MentionsList.xlsx";
            string twitterIDPattern = @"@\w{1,15}";
            MatchCollection twitterIDMatch = Regex.Matches(message.MessageBody, twitterIDPattern);

            try
            {
                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    ExcelWorksheet mentionsList = package.Workbook.Worksheets[0];

                    for (int i = 1; i < twitterIDMatch.Count; i++)
                    {
                        int rowCount = mentionsList.Dimension.Rows;
                        bool contains = false;
                        for (int row = 1; row <= rowCount; row++)
                        {
                            if (mentionsList.Cells[row, 1].Text == twitterIDMatch[i].Value)
                            {
                                int value = Int32.Parse(mentionsList.Cells[row, 2].Text) + 1;
                                mentionsList.Cells[row, 2].Value = value;
                                package.Save();
                                contains = true;
                                break;
                            }
                        }
                        if (contains == false)
                        {
                            mentionsList.Cells[rowCount + 1, 1].Value = twitterIDMatch[i].Value;
                            mentionsList.Cells[rowCount + 1, 2].Value = 1;
                            package.Save();
                        }
                    }
                    package.Save();
                }
            }
            catch
            {
                mainWindow.CloseFile();
                AddToMentionList(message);
            }

        }
        // Adds hashtags found in the text to trending list in excel file
        public void AddToTrendingList(MessageModel message)
        {
            string filePath = "../../../History/TrendingList.xlsx";
            string hashtagPattern = @"#\w{1,20}";
            MatchCollection hashtagMatch = Regex.Matches(message.MessageBody, hashtagPattern);
            try
            {
                using (ExcelPackage package = new ExcelPackage(filePath))
                {
                    ExcelWorksheet trendingList = package.Workbook.Worksheets[0];

                    for (int i = 0; i < hashtagMatch.Count; i++)
                    {
                        int rowCount = trendingList.Dimension.Rows;
                        bool contains = false;
                        for (int row = 1; row <= rowCount; row++)
                        {
                            if (trendingList.Cells[row, 1].Text == hashtagMatch[i].Value)
                            {
                                int value = Int32.Parse(trendingList.Cells[row, 2].Text) + 1;
                                trendingList.Cells[row, 2].Value = value;
                                package.Save();
                                contains = true;
                                break;
                            }
                        }
                        if (contains == false)
                        {
                            trendingList.Cells[rowCount + 1, 1].Value = hashtagMatch[i].Value;
                            trendingList.Cells[rowCount + 1, 2].Value = 1;
                            package.Save();
                        }
                    }
                    package.Save();
                }
            }
            catch
            {
                mainWindow.CloseFile();
                AddToTrendingList(message);
            }
        }

        // Saves message to json file depending on its type
        public void SaveMessage(MessageModel message)
        {
            if(message is SMSMessageModel smsMessage)
            {
                string smsFilePath = "../../../History/SMSMessages.json";
                string smsExistingJsonContent = File.ReadAllText(smsFilePath);
                var smsArray = JsonConvert.DeserializeObject<List<SMSMessageModel>>(smsExistingJsonContent);
                smsArray.Add(smsMessage);
                string smsJsonString = JsonConvert.SerializeObject(smsArray, Formatting.Indented);
                File.WriteAllText(smsFilePath, smsJsonString);
                MessageBox.Show("Finished");
            }
            else if (message is EmailMessageModel emailMessage)
            {
                string emailFilePath = "../../../History/EmailMessages.json";
                string emailExistingJsonContent = File.ReadAllText(emailFilePath);
                var emailArray = JsonConvert.DeserializeObject<List<EmailMessageModel>>(emailExistingJsonContent);
                emailArray.Add(emailMessage);
                string emailJsonString = JsonConvert.SerializeObject(emailArray, Formatting.Indented);
                File.WriteAllText(emailFilePath, emailJsonString);
                MessageBox.Show("Finished");
            }
            else if (message is TweetMessageModel tweetMessage)
            {
                string tweetFilePath = "../../../History/TweetMessages.json";
                string tweetExistingJsonContent = File.ReadAllText(tweetFilePath);
                var tweetArray = JsonConvert.DeserializeObject<List<TweetMessageModel>>(tweetExistingJsonContent);
                tweetArray.Add(tweetMessage);
                string emailJsonString = JsonConvert.SerializeObject(tweetArray, Formatting.Indented);
                File.WriteAllText(tweetFilePath, emailJsonString);
                MessageBox.Show("Finished");
            }
        }
    }
}
