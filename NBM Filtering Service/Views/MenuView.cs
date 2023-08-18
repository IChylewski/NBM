using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NBM_Filtering_Service.Views
{
    internal class MenuView
    {

        // All methods in this class handle changing views

        private MainWindow mainWindow;
        public MenuView(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
        public void EnableMenuView()
        {
            mainWindow.MessageInputView.Visibility = Visibility.Collapsed;
            mainWindow.MessageLoadFileView.Visibility = Visibility.Collapsed;
            mainWindow.MessageReaderView.Visibility = Visibility.Collapsed;
            mainWindow.SummaryView.Visibility = Visibility.Collapsed;
            mainWindow.MenuView.Visibility = Visibility.Visible;
        }
        public void EnableSendMessageView()
        {
            mainWindow.MenuView.Visibility = Visibility.Collapsed;
            mainWindow.MessageLoadFileView.Visibility = Visibility.Collapsed;
            mainWindow.MessageReaderView.Visibility = Visibility.Collapsed;
            mainWindow.SummaryView.Visibility = Visibility.Collapsed;
            mainWindow.MessageInputView.Visibility = Visibility.Visible;
        }
        public void EnableLoadFileView()
        {
            mainWindow.MenuView.Visibility = Visibility.Collapsed;
            mainWindow.MessageInputView.Visibility = Visibility.Collapsed;
            mainWindow.SummaryView.Visibility = Visibility.Collapsed;
            mainWindow.MessageReaderView.Visibility = Visibility.Collapsed;
            mainWindow.MessageLoadFileView.Visibility = Visibility.Visible;
        }
        public void EnableReaderView()
        {
            mainWindow.MenuView.Visibility = Visibility.Collapsed;
            mainWindow.MessageInputView.Visibility = Visibility.Collapsed;
            mainWindow.SummaryView.Visibility = Visibility.Collapsed;
            mainWindow.MessageLoadFileView.Visibility = Visibility.Collapsed;
            mainWindow.MessageReaderView.Visibility = Visibility.Visible;
        }
        public void EnableSummaryView()
        {
            mainWindow.MenuView.Visibility = Visibility.Collapsed;
            mainWindow.MessageInputView.Visibility = Visibility.Collapsed;
            mainWindow.MessageLoadFileView.Visibility = Visibility.Collapsed;
            mainWindow.MessageReaderView.Visibility = Visibility.Collapsed;
            mainWindow.SummaryView.Visibility = Visibility.Visible;
        }
    }
}
