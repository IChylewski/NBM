using NBM_Filtering_Service;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace UnitTests.Processors
{
    internal static class SendMessageViewProcessors
    {

        public static bool ValidateMessage_Processor(MessageModel message)
        {
            List<string> possibleNOT = new List<string>()
            {
                "[Theft]","[Staff Attack]","[Raid]","[Customer Attack]", "[Staff Abuse]", "[Bomb Threat]", "[Terrorism]", "[Suspicious Incident]", "[Intelligence]", "[Cash Loss]"
            };
            bool validated = true;

            if (message.MessageID.Length == 0)
            {
                validated = false;
                return validated;
            }
            switch (message.MessageID[0])
            {
                case 'S':
                    if (message.MessageID.Length < 10 || message.MessageID.Length > 10)
                    {
                        validated = false;
                    }

                    string phoneNumberPattern = @"\b(\+\d{1,3}[-\s]?)?(\(\d{1,}\)[-.\s]?)?\d{1,}[-.\s]?\d{1,}[-.\s]?\d{1,}\b";
                    Match match = Regex.Match(message.MessageBody, phoneNumberPattern);

                    if (!match.Success)
                    {
                        validated = false;
                        return validated;
                    }

                    if (message.MessageBody.Length < 20 || message.MessageBody.Length > 140)
                    {
                        validated = false;
                    }
                    return validated;
                case 'E':

                    if (message.MessageID.Length < 10 || message.MessageID.Length > 10)
                    {
                        validated = false;
                    }

                    string emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
                    string subjectPattern = @"(?<=Subject:\s)\[(\w{1,20})(?:\s+(\w{1,20}))*\]";
                    string sirPattern = @"\bSIR\s\d{2}/\d{2}/\d{4}\b";
                    string sortcodePattern = @"(?<=Sort-Code:\s)\[[\w\d\-]{8}\]";
                    string natureOfIncidentPattern = @"(?<=Nature-of-Incident:\s)\[(\w{1,20})(?:\s+(\w{5,30}))*\]";

                    Match emailMatch = Regex.Match(message.MessageBody, emailPattern, RegexOptions.IgnoreCase);
                    Match subjectMatch = Regex.Match(message.MessageBody, subjectPattern, RegexOptions.IgnoreCase);
                    Match sirMatch = Regex.Match(message.MessageBody, sirPattern, RegexOptions.IgnoreCase);
                    Match sortcodeMatch = Regex.Match(message.MessageBody, sortcodePattern, RegexOptions.IgnoreCase);
                    Match natureOfIncidentMatch = Regex.Match(message.MessageBody, natureOfIncidentPattern, RegexOptions.IgnoreCase);

                    if (!emailMatch.Success)
                    {
                        validated = false;
                        return validated;
                    }
                    if (!subjectMatch.Success)
                    {
                        validated = false;
                        return validated;
                    }
                    if (sirMatch.Success)
                    {
                        if (!natureOfIncidentMatch.Success)
                        {
                            validated = false;
                            return validated;
                        }
                        if (!sortcodeMatch.Success)
                        {
                            validated = false;
                            return validated;
                        }
                        if (!possibleNOT.Contains(natureOfIncidentMatch.Value))
                        {
                            validated = false;
                            return validated;
                        }
                    }
                    // checking text length
                    int followingTextStartIndex = subjectMatch.Index + subjectMatch.Length;
                    int remainingLength = message.MessageBody.Substring(followingTextStartIndex).Length;
                    if (remainingLength > 1028 || remainingLength < 20)
                    {
                        validated = false;
                    }
                    return validated;
                case 'T':

                    if (message.MessageID.Length < 10 || message.MessageID.Length > 10)
                    {
                        validated = false;
                    }

                    string twitterIDPattern = @"@\w{1,15}";
                    Match twitterIDMatch = Regex.Match(message.MessageBody, twitterIDPattern);

                    if (!twitterIDMatch.Success)
                    {
                        validated = false;
                        return validated;
                    }

                    int followingTextStartIndexTweet = twitterIDMatch.Index + twitterIDMatch.Length;
                    int remainingLengthTweet = message.MessageBody.Substring(followingTextStartIndexTweet).Length;
                    if (remainingLengthTweet > 140 || remainingLengthTweet < 5)
                    {
                        validated = false;
                    }

                    return validated;
                default:
                    if (message.MessageID[0] != 'S' || message.MessageID[0] != 'E' || message.MessageID[0] != 'T')
                    {
                        validated = false;
                    }
                    return validated;
            }
        }
        public static Dictionary<string, string> LoadAbbreviations_Processor(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            Dictionary<string, string> abbreviationDictionary = new Dictionary<string, string>();
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
            return abbreviationDictionary;
        }
        public static SMSMessageModel ProcessSMSMessage_Processor(MessageModel message, Dictionary<string,string> abbreviations)
        {
            message.MessageText = message.MessageBody;

            MatchCollection matches = Regex.Matches(message.MessageBody, @"\b[A-Z0-9!@#$%^&*()-_=+{}\[\]:;""'<>,.?/\\|]+\b");
            HashSet<string> uniqueMatches = new HashSet<string>();

            foreach (Match match in matches)
            {
                string abbreviation = match.Value.ToUpper();
                uniqueMatches.Add(abbreviation);
            }

            foreach (string abbreviation in uniqueMatches)
            {
                if (abbreviations.TryGetValue(abbreviation, out string meaning))
                {
                    string pattern = $@"\b{Regex.Escape(abbreviation)}\b";
                    message.MessageText = Regex.Replace(message.MessageText, pattern, $"{abbreviation} <{meaning}>", RegexOptions.IgnoreCase);
                }
            }

            string phoneNumberPattern = @"(?<=Phone-Number:\s)\[([^\]]{7,25})\]";
            Match phoneNumberMatch = Regex.Match(message.MessageBody, phoneNumberPattern);

            string[] wordsToRemove = { "Phone-Number: ", phoneNumberMatch.Value };
            message.MessageText = RemoveWordAndSpaces_Processor(message.MessageText, wordsToRemove);

            SMSMessageModel smsMessage = new SMSMessageModel(message.MessageID, message.MessageBody, phoneNumberMatch.Groups[1].Value, message.MessageText);

            return smsMessage;
        }
        public static EmailMessageModel ProcessEmailMessage_Processor(MessageModel message)
        {
            message.MessageText = message.MessageBody;

            string urlPattern = @"(https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})";

            MatchCollection matches = Regex.Matches(message.MessageText, urlPattern);
            HashSet<string> uniqueMatches = new HashSet<string>();

            foreach (Match match in matches)
            {
                string link = match.Value;
                uniqueMatches.Add(link);
            }

            foreach (string link in uniqueMatches)
            {
                string pattern = $@"\b{Regex.Escape(link)}\b";
                message.MessageText = Regex.Replace(message.MessageText, pattern, $"<URL Quarantined>");
                //AddToQuarantine_Processor(link, quarantineFilePath);
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

            if (sirMatch.Success)
            {
                type = "SIR";
                wordsToRemove = new string[] { emailMatch.Value, sirMatch.Value, "Subject: ", subjectMatch.Value, "Sort-Code: ", sortcodeMatch.Value, "Nature-of-Incident: ", notMatch.Value };
                //AddToSIRList_Processor(sirMatch.Value, notMatch.Value, sirFilePath);
            }
            message.MessageText = RemoveWordAndSpaces_Processor(message.MessageText, wordsToRemove);

            EmailMessageModel emailMessage = new EmailMessageModel(message.MessageID, message.MessageBody, emailMatch.Value, sirMatch.Value, type, subjectMatch.Groups[1].Value, sortcodeMatch.Groups[1].Value, notMatch.Groups[1].Value);
            emailMessage.MessageText = message.MessageText;

            return emailMessage;
        }
        public static TweetMessageModel ProcessTweetMessage_Processor(MessageModel message, Dictionary<string, string> abbreviations)
        {
            message.MessageText = message.MessageBody;

            MatchCollection matches = Regex.Matches(message.MessageBody, @"\b[A-Z0-9!@#$%^&*()-_=+{}\[\]:;""'<>,.?/\\|]+\b");
            HashSet<string> uniqueMatches = new HashSet<string>();

            foreach (Match match in matches)
            {
                string abbreviation = match.Value.ToUpper();
                uniqueMatches.Add(abbreviation);
            }

            foreach (string abbreviation in uniqueMatches)
            {
                if (abbreviations.TryGetValue(abbreviation, out string meaning))
                {
                    string pattern = $@"\b{Regex.Escape(abbreviation)}\b";
                    message.MessageText = Regex.Replace(message.MessageText, pattern, $"{abbreviation} <{meaning}>", RegexOptions.IgnoreCase);
                }
            }

            string twitterIDPattern = @"@\w{1,15}";
            Match twitterIDMatch = Regex.Match(message.MessageBody, twitterIDPattern);

            string[] wordsToRemove = { twitterIDMatch.Value };
            message.MessageText = RemoveWordAndSpaces_Processor(message.MessageText, wordsToRemove);

            TweetMessageModel tweetMessage = new TweetMessageModel(message.MessageID, message.MessageBody, twitterIDMatch.Value, message.MessageText);

            return tweetMessage;
        }
        public static void AddToQuarantine_Processor(string url, string quarantineFilePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage(quarantineFilePath))
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
        public static void AddToSIRList_Processor(string sir, string natureOfIncident, string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

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
        public static void AddToMentionList_Processor(MessageModel message ,string filePath)
        {
            string twitterIDPattern = @"@\w{1,15}";
            MatchCollection twitterIDMatch = Regex.Matches(message.MessageBody, twitterIDPattern);

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
        public static void AddToTrendingList_Processor(MessageModel message, string filePath)
        {
            string hashtagPattern = @"#\w{1,20}";
            MatchCollection hashtagMatch = Regex.Matches(message.MessageBody, hashtagPattern);

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
        public static string RemoveWordAndSpaces_Processor(string input, string[] wordsToRemove)
        {
            string pattern = string.Join("|", wordsToRemove.Select(Regex.Escape));
            string result = Regex.Replace(input, pattern, string.Empty);
            result = Regex.Replace(result, @"\s+", " ").Trim();
            return result;
        }
        // Here SaveMessage 
    }
}
