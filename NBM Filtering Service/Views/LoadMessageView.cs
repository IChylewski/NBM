using Microsoft.Win32;
using NBM_Filtering_Service.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NBM_Filtering_Service.Views
{
    public static class LoadMessageView
    {
        // This method opens file dialog returns path of selected file
        public static string BrowseFiles()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "C:\\";
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            if(openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                return selectedFilePath;
            }
            return "";
        }
        // Loads text from the file and return it as Message Model
        public static MessageModel LoadMessage(string filePath)
        {
            string? messageID = File.ReadLines(filePath).FirstOrDefault(); // First line of the message in the input text file is ID 

            string[] lines = File.ReadAllLines(filePath);
            string restOfText = string.Join(Environment.NewLine, lines.Skip(1)); // Skip the first line (ID)

            return new MessageModel(messageID, restOfText);
        }
    }
}
