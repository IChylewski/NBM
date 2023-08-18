using Microsoft.Win32;
using NBM_Filtering_Service.Models;
using NBM_Filtering_Service.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NBM_Filtering_Service
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MenuView menuView;
        private SendMessageView sendMessageView;
        private SummaryView summaryView;
        private ReaderView readerView;
        private int messageCounter = 0;
        public MainWindow()
        {
            menuView = new MenuView(this);
            sendMessageView = new SendMessageView(this);
            summaryView = new SummaryView(this);
            readerView = new ReaderView(this);
            InitializeComponent();
        }
        private void SendMessageViewBtn_Click(object sender, RoutedEventArgs e)
        {
            menuView.EnableSendMessageView();
        }
        private void LoadFileViewBtn_Click(object sender, RoutedEventArgs e)
        {
            menuView.EnableLoadFileView();
        }
        private void ReadMessagesViewBtn_Click(object sender, RoutedEventArgs e)
        {
            menuView.EnableReaderView();
            readerView.LoadAllMessages();
            readerView.InitializeReader("Sms");
        }
        private void BackToMenuBtn_Click(object sender, RoutedEventArgs e)
        {
            menuView.EnableMenuView();
        }
        private void SendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            string messageID = MessageIDTextBox.Text;
            string messageBody = MessageTextBox.Text;
            MessageModel message = new MessageModel(messageID, messageBody);
            sendMessageView.VSCMessage(message);
        }
        private void ShowSummaryBtn_Click(object sender, RoutedEventArgs e)
        {
            menuView.EnableSummaryView();
            TrendingList.ItemsSource = summaryView.LoadTrendingList();
            MentionsList.ItemsSource = summaryView.LoadMentionsList();
            SirList.ItemsSource = summaryView.LoadSIRList();
        }
        private void BrowseFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            FilePathTxtBlk.Text = LoadMessageView.BrowseFiles();
        }
        private void SendLoadedMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageModel message = LoadMessageView.LoadMessage(FilePathTxtBlk.Text);
            menuView.EnableSendMessageView();
            MessageIDTextBox.Text = message.MessageID;
            MessageTextBox.Text = message.MessageBody;
            //sendMessageView.VSCMessage(message);
        }
        private void ReaderMessageType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MessageTypeReaderCmbBox.SelectedItem != null && MessageReaderView.Visibility == Visibility.Visible)
            {
                string? selectedOption = ((ComboBoxItem)MessageTypeReaderCmbBox.SelectedItem).Content.ToString();
                
                switch(selectedOption)
                {
                    case "SMS Message":
                        {
                            messageCounter = 0;
                            EmailReader.Visibility = Visibility.Collapsed;
                            TweetReader.Visibility = Visibility.Collapsed;
                            SMSReader.Visibility = Visibility.Visible;
                            readerView.InitializeReader("Sms");
                            break;
                        }
                    case "Email Message":
                        {
                            messageCounter = 0;
                            TweetReader.Visibility = Visibility.Collapsed;
                            SMSReader.Visibility = Visibility.Collapsed;
                            EmailReader.Visibility = Visibility.Visible;
                            readerView.InitializeReader("Email");
                            break;
                        }
                    case "Tweet Message":
                        {
                            messageCounter = 0;
                            EmailReader.Visibility = Visibility.Collapsed;
                            SMSReader.Visibility = Visibility.Collapsed;
                            TweetReader.Visibility = Visibility.Visible;
                            readerView.InitializeReader("Tweet");
                            break;
                        }
                }
            }
        }
        private void NextMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageTypeReaderCmbBox.SelectedItem != null && MessageReaderView.Visibility == Visibility.Visible)
            {
                string? selectedOption = ((ComboBoxItem)MessageTypeReaderCmbBox.SelectedItem).Content.ToString();

                switch (selectedOption)
                {
                    case "SMS Message":
                        {
                            if (messageCounter >= readerView.smsMessages.Count - 1)
                            {
                                messageCounter = readerView.smsMessages.Count - 1;
                                return;
                            }
                            messageCounter += 1;
                            readerView.NextMessage("Sms", messageCounter);
                            break;
                        }
                    case "Email Message":
                        {
                            if (messageCounter >= readerView.emailMessages.Count - 1)
                            {
                                messageCounter = readerView.emailMessages.Count - 1;
                                return;
                            }
                            messageCounter += 1;
                            readerView.NextMessage("Email", messageCounter);
                            break;
                        }
                    case "Tweet Message":
                        {
                            if (messageCounter >= readerView.tweetMessages.Count - 1)
                            {
                                messageCounter = readerView.tweetMessages.Count - 1;
                                return;
                            }
                            messageCounter += 1;
                            readerView.NextMessage("Tweet", messageCounter);
                            break;
                        }
                }
            }
        }
        private void PreviousMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MessageTypeReaderCmbBox.SelectedItem != null && MessageReaderView.Visibility == Visibility.Visible)
            {
                string? selectedOption = ((ComboBoxItem)MessageTypeReaderCmbBox.SelectedItem).Content.ToString();
                if(messageCounter <= 0)
                {
                    messageCounter = 0;
                    return;
                }

                switch (selectedOption)
                {
                    case "SMS Message":
                        {
                            messageCounter -= 1;
                            readerView.PreviousMessage("Sms", messageCounter);
                            break;
                        }
                    case "Email Message":
                        {
                            messageCounter -= 1;
                            readerView.PreviousMessage("Email", messageCounter);
                            break;
                        }
                    case "Tweet Message":
                        {
                            messageCounter -= 1;
                            readerView.PreviousMessage("Tweet", messageCounter);
                            break;
                        }
                }
            }
        }
        public void CloseFile()
        {
            try
            {
                // Get a list of all processes that have the file open
                Process[] processes = Process.GetProcesses();
                foreach (Process process in processes)
                {
                    try
                    {
                        if (process.ProcessName == "EXCEL")
                            process.Kill();
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions (e.g., access denied, process already exited)
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }
}
