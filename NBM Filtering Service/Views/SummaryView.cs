using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NBM_Filtering_Service.Views
{
    internal class SummaryView
    {
        private MainWindow mainWindow;

        public SummaryView(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        // Loads trending list from excel file and returns it as dictionary
        public Dictionary<string, int> LoadTrendingList()
        {
            Dictionary<string, int> trendingList = new Dictionary<string, int>();
            string filePath = "../../../History/TrendingList.xlsx";
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        string hashtag = worksheet.Cells[row, 1].Text;
                        int value = Int32.Parse(worksheet.Cells[row, 2].Text);

                        trendingList.Add(hashtag, value);
                    }
                }
            }
            catch
            {
                mainWindow.CloseFile();
                return LoadTrendingList();
            }

            trendingList = trendingList.OrderByDescending(pair => pair.Value).ToDictionary(x=> x.Key, x=> x.Value);
            return trendingList;
        }
        // Loads mentions list from the excel file and returns it as dictionary
        public Dictionary<string, int> LoadMentionsList()
        {
            Dictionary<string, int> mentionList = new Dictionary<string, int>();
            string filePath = "../../../History/MentionsList.xlsx";
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        string mention = worksheet.Cells[row, 1].Text;
                        int value = Int32.Parse(worksheet.Cells[row, 2].Text);

                        mentionList.Add(mention, value);
                    }
                }
            }
            catch
            {
                mainWindow.CloseFile();
                return LoadMentionsList(); 
            }
            mentionList = mentionList.OrderByDescending(pair => pair.Value).ToDictionary(x => x.Key, x => x.Value);
            return mentionList;
        }
        // Loads SIR List from excel file and returns it as dictionary
        public Dictionary<string, string> LoadSIRList()
        {
            Dictionary<string, string> sirList = new Dictionary<string, string>();
            string filePath = "../../../History/SIRList.xlsx";
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                    {
                        string mention = worksheet.Cells[row, 1].Text;
                        string value = worksheet.Cells[row, 2].Text;

                        sirList.Add(mention, value);
                    }
                }
            }
            catch
            {
                mainWindow.CloseFile();
                return LoadSIRList();
            }

            return sirList;
        }
    }
}
