namespace UnitTests
{
    public class LoadMessageViewTests
    {
        [Test]
        public void Loading_File_Returns_Message()
        {
            // ARRANGE
            string testFilePath = "../../../TestingData/TestSMSInputMessage.txt";

            MessageModel expectedResult = new MessageModel(
                "S123123123",
                "+447723231243 This is test SMS message ROTFL.");
            // ACT
            //LoadMessageView loadMessageView = new LoadMessageView();

            MessageModel actualResult = LoadMessageView.LoadMessage(testFilePath);

            // ASSERT
            Assert.AreEqual(expectedResult.MessageID, actualResult.MessageID);
            Assert.AreEqual(expectedResult.MessageBody, actualResult.MessageBody);
        }
    }
}