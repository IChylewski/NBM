using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Moq;
using NBM_Filtering_Service;
using OfficeOpenXml;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Windows;
using UnitTests.Processors;
using LicenseContext = OfficeOpenXml.LicenseContext;

namespace UnitTests
{
    public class SendMesssageViewTests
    {
     
        [Test]
        public void Validate_Message_Test()
        {
            // ARRANGE
            MessageModel trueMessageOne = new MessageModel(
                "S123123123",
                "+447723231243 This is test SMS message ROTFL.");
            MessageModel falseMessageOne = new MessageModel(
                "S12312312",
                "+447723231243 This is test SMS message ROTFL.");
            // ACT
            //var mockDependency = new Mock<MainWindow>();
           

            bool actualResultTrueOne = SendMessageViewProcessors.ValidateMessage_Processor(trueMessageOne);
            bool actualResultFalseOne = SendMessageViewProcessors.ValidateMessage_Processor(falseMessageOne);


            // ASSERT
            Assert.IsTrue(actualResultTrueOne);
            Assert.IsFalse(actualResultFalseOne);
        }
        [Test]
        public void Load_Abbrevations_Test()
        {
            // ARRANGE
            Dictionary<string, string> expectedResult = new Dictionary<string, string>
            {
                { "AAP", "Always a pleasure" },
                { "AAR", "At any rate" },
                { "AAS", "Alive and smiling" },
                { "ADN", "Any day now" },
                { "AEAP", "As early as possible" },
                { "AFAIK", "As far as I know" },
                { "AFK", "Away from keyboard" },
                { "AKA", "Also known as" },
                { "AISB", "As it should be" },
                { "AOTA", "All of the above" }
            };
            string filePath = "../../../TestingData/Test_Abbreviations.xlsx";
            // ACT

            Dictionary<string, string> actualResult = SendMessageViewProcessors
                .LoadAbbreviations_Processor(filePath);

            // ASSERT
            Assert.AreEqual(expectedResult, actualResult);
        }
        [Test]
        public void Process_SMS_Message_Test()
        {
            // ARRANGE
            MessageModel messageInput = new MessageModel(
                "S123123123",
                "Phone-Number: [+447746242473] This is test SMS message B4N."
                );
            SMSMessageModel expectedResult = new SMSMessageModel(
                "S123123123",
                "Phone-Number: [+447746242473] This is test SMS message B4N.",
                "+447746242473",
                "This is test SMS message B4N <Bye for now>."
                );

            Dictionary<string, string> abbreviations = new Dictionary<string, string>
            {
                { "AAP", "Always a pleasure" },
                { "AAR", "At any rate" },
                { "AAS", "Alive and smiling" },
                { "ADN", "Any day now" },
                { "AEAP", "As early as possible" },
                { "AFAIK", "As far as I know" },
                { "AFK", "Away from keyboard" },
                { "AKA", "Also known as" },
                { "AISB", "As it should be" },
                { "AOTA", "All of the above" },
                { "B4N", "Bye for now" }
            };
            // ACT

            SMSMessageModel actualResult = SendMessageViewProcessors.ProcessSMSMessage_Processor(messageInput, abbreviations);

            // ASSERT
            Assert.AreEqual(expectedResult.MessageID, actualResult.MessageID);
            Assert.AreEqual(expectedResult.MessageBody, actualResult.MessageBody);
            Assert.AreEqual(expectedResult.PhoneNumber, actualResult.PhoneNumber);
            Assert.AreEqual(expectedResult.MessageText, actualResult.MessageText);
        }
        [Test]
        public void Process_Email_Message_Test()
        {
            // ARRANGE
            MessageModel messageInput = new MessageModel(
                "E123123123",
                "semirek2301@gmail.com SIR 12/03/2023 Nature-of-Incident: [Bomb Threat] Sort-Code: [88-88-88] Subject: [xyz xyz] test test www.google.com http:\\www.anywhere.com test test"
                );
            EmailMessageModel expectedResult = new EmailMessageModel(
                "E123123123",
                "semirek2301@gmail.com SIR 12/03/2023 Nature-of-Incident: [Bomb Threat] Sort-Code: [88-88-88] Subject: [xyz xyz] test test www.google.com http:\\www.anywhere.com test test",
                "semirek2301@gmail.com",
                "SIR 12/03/2023",
                "SIR",
                "xyz xyz",
                "88-88-88",
                "Bomb Threat",
                "test test <URL Quarantined> http:\\<URL Quarantined> test test"
                );

            //string quarantineFilePath = "../../../TestingData/Test_QuarantineList.xlsx";
            //string sirFilePath = "../../../TestingData/Test_SIRList.xlsx";


            // ACT

            EmailMessageModel actualResult = SendMessageViewProcessors.ProcessEmailMessage_Processor(messageInput);

            // ASSERT
            Assert.AreEqual(expectedResult.MessageID, actualResult.MessageID);
            Assert.AreEqual(expectedResult.MessageBody, actualResult.MessageBody);
            Assert.AreEqual(expectedResult.Email, actualResult.Email);
            Assert.AreEqual(expectedResult.SIR, actualResult.SIR);
            Assert.AreEqual(expectedResult.NatureOfIncident, actualResult.NatureOfIncident);
            Assert.AreEqual(expectedResult.Type, actualResult.Type);
            Assert.AreEqual(expectedResult.Subject, actualResult.Subject);
            Assert.AreEqual(expectedResult.MessageText, actualResult.MessageText);
        }
        [Test]
        public void Process_Tweet_Message_Test()
        {
            // ARRANGE
            MessageModel messageInput = new MessageModel(
                "T123123123",
                "@SENDErID beka beka beka B4N @MentionOne @Mention #HashtagTest beka beka"
                );
            TweetMessageModel expectedResult = new TweetMessageModel(
                "T123123123",
                "@SENDErID beka beka beka B4N @MentionOne @Mention #HashtagTest beka beka",
                "@SENDErID",
                "beka beka beka B4N <Bye for now> @MentionOne @Mention #HashtagTest beka beka"
                );

            Dictionary<string, string> abbreviations = new Dictionary<string, string>
            {
                { "AAP", "Always a pleasure" },
                { "AAR", "At any rate" },
                { "AAS", "Alive and smiling" },
                { "ADN", "Any day now" },
                { "AEAP", "As early as possible" },
                { "AFAIK", "As far as I know" },
                { "AFK", "Away from keyboard" },
                { "AKA", "Also known as" },
                { "AISB", "As it should be" },
                { "AOTA", "All of the above" },
                { "B4N", "Bye for now" }
            };
            // ACT

            TweetMessageModel actualResult = SendMessageViewProcessors.ProcessTweetMessage_Processor(messageInput, abbreviations);

            // ASSERT
            Assert.AreEqual(expectedResult.MessageID, actualResult.MessageID);
            Assert.AreEqual(expectedResult.MessageBody, actualResult.MessageBody);
            Assert.AreEqual(expectedResult.TwitterID, actualResult.TwitterID);
            Assert.AreEqual(expectedResult.MessageText, actualResult.MessageText);
        }
        [Test]
        public void Add_To_Quarantine_Test()
        {
            // ARRANGE
            string quarantineFilePath = "../../../TestingData/Test_QuarantineList.xlsx";
            string urlToAdd = "www.google.com";
            string expectedResult = urlToAdd;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // ACT
            SendMessageViewProcessors
                .AddToQuarantine_Processor(urlToAdd, quarantineFilePath);

            string actualResult = "";

            using (ExcelPackage package = new ExcelPackage(quarantineFilePath))
            {
                ExcelWorksheet quarantineList = package.Workbook.Worksheets[0];
                int rowCount = quarantineList.Dimension.Rows;

                actualResult = quarantineList.Cells[rowCount, 1].Text;
            }

            // ASSERT
            Assert.AreEqual(expectedResult, actualResult);

        }
        [Test]
        public void Add_To_SIR_List_Test()
        {
            // ARRANGE
            string sirFilePath = "../../../TestingData/Test_SIRList.xlsx";
            string sirToAdd = "SIR 23/09/2020";
            string notToAdd = "[Theft]";
            
            string expectedSIRResult = sirToAdd;
            string expectedNotResult = notToAdd;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // ACT
            SendMessageViewProcessors
                .AddToSIRList_Processor(sirToAdd, notToAdd, sirFilePath);

            string actualSirResult = "";
            string actualNotResult = "";

            using (ExcelPackage package = new ExcelPackage(sirFilePath))
            {
                ExcelWorksheet sirList = package.Workbook.Worksheets[0];
                int rowCount = sirList.Dimension.Rows;

                actualSirResult = sirList.Cells[rowCount, 1].Text;
                actualNotResult = sirList.Cells[rowCount, 2].Text;
            }

            // ASSERT
            Assert.AreEqual(expectedSIRResult, actualSirResult);
            Assert.AreEqual(expectedNotResult, actualNotResult);

        }
        [Test]
        public void Add_To_Mentions_List_Test()
        {
            // ARRANGE
            string filePath = "../../../TestingData/Test_MentionsList.xlsx";
            MessageModel messageInput = new MessageModel(
                "T123123123",
                "@TwitterID This is test twitter @about different @things @test");

            Dictionary<string, int> currentValues = new Dictionary<string, int>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string twitterIDPattern = @"@\w{1,15}";
            MatchCollection twitterIDMatch = Regex.Matches(messageInput.MessageBody, twitterIDPattern);

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
                            currentValues.Add(mentionsList.Cells[row, 1].Text, Int32.Parse(mentionsList.Cells[row, 2].Text));
                            contains = true;
                            break;
                        }
                    }
                    if (contains == false)
                    {
                        currentValues.Add(twitterIDMatch[i].Value, 0);
                    }
                }
            }

            Dictionary<string, int> expectedResult = new Dictionary<string, int>();

            foreach (var entry in currentValues)
            {
                expectedResult.Add(entry.Key, entry.Value + 1);
            }
            expectedResult = expectedResult.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            /*expectedResult = new Dictionary<string, int>()
            {
                { "@about", 3},
                { "@test", 1 }

            };*/
            // ACT

            SendMessageViewProcessors.AddToMentionList_Processor(messageInput, filePath);

            Dictionary<string, int> actualResults = new Dictionary<string, int>();

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
                            actualResults.Add(mentionsList.Cells[row, 1].Text, Int32.Parse(mentionsList.Cells[row, 2].Text));
                            contains = true;
                            break;
                        }
                    }
                    if (contains == false)
                    {
                        currentValues.Add(twitterIDMatch[i].Value, 0);
                    }
                }
            }
            actualResults = actualResults.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // ASSERT
            Assert.AreEqual(expectedResult, actualResults);

        }
        [Test]
        public void Add_To_Trending_List_Test()
        {
            // ARRANGE
            string filePath = "../../../TestingData/Test_TrendingList.xlsx";
            MessageModel messageInput = new MessageModel(
                "T123123123",
                "@twitterID This is test twitter @about different #things #test");

            Dictionary<string, int> currentValues = new Dictionary<string, int>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string twitterIDPattern = @"#\w{1,15}";
            MatchCollection hashtagMatch = Regex.Matches(messageInput.MessageBody, twitterIDPattern);

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
                            currentValues.Add(trendingList.Cells[row, 1].Text, Int32.Parse(trendingList.Cells[row, 2].Text));
                            contains = true;
                            break;
                        }
                    }
                    if (contains == false)
                    {
                        currentValues.Add(hashtagMatch[i].Value, 0);
                    }
                }
            }

            Dictionary<string, int> expectedResult = new Dictionary<string, int>();

            foreach (var entry in currentValues)
            {
                expectedResult.Add(entry.Key, entry.Value + 1);
            }
            expectedResult = expectedResult.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            /*expectedResult = new Dictionary<string, int>()
            {
                { "#about", 3},
                { "#test", 1 }

            };*/
            // ACT

            SendMessageViewProcessors.AddToTrendingList_Processor(messageInput, filePath);

            Dictionary<string, int> actualResults = new Dictionary<string, int>();

            using (ExcelPackage package = new ExcelPackage(filePath))
            {
                ExcelWorksheet mentionsList = package.Workbook.Worksheets[0];

                for (int i = 0; i < hashtagMatch.Count; i++)
                {
                    int rowCount = mentionsList.Dimension.Rows;
                    bool contains = false;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        if (mentionsList.Cells[row, 1].Text == hashtagMatch[i].Value)
                        {
                            actualResults.Add(mentionsList.Cells[row, 1].Text, Int32.Parse(mentionsList.Cells[row, 2].Text));
                            contains = true;
                            break;
                        }
                    }
                    if (contains == false)
                    {
                        currentValues.Add(hashtagMatch[i].Value, 0);
                    }
                }
            }
            actualResults = actualResults.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            // ASSERT
            Assert.AreEqual(expectedResult, actualResults);

        }
    }
}