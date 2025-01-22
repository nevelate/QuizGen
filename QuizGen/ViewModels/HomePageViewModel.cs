using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NanoXLSX.Exceptions;
using QuizGen.TestParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;

namespace QuizGen.ViewModels
{
    public partial class HomePageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? filePath;
        [ObservableProperty]
        private string? testName;
        [ObservableProperty]
        private string? testDescription;

        [ObservableProperty]
        private int testRangeFrom;
        [ObservableProperty]
        private int testRangeTo;
        [ObservableProperty]
        private int testGrouping;

        [ObservableProperty]
        private string? testInfo;
        [ObservableProperty]
        private string? overallInfo;

        private ITestParser testParser = null!;
        public TopLevel? TopLevel;

        public HomePageViewModel()
        {
            
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            if(TopLevel != null)
            {
                var storage = TopLevel.StorageProvider;
                var defaultLocation = await storage.TryGetWellKnownFolderAsync(WellKnownFolder.Downloads);
                var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions() 
                {
                    AllowMultiple = false,
                    SuggestedStartLocation = defaultLocation
                });

                if (files.Any()) FilePath = files.First().Path.AbsolutePath;
            }
        }

        [RelayCommand]
        private void CheckTests()
        {
            testParser = new LMSTestParser();

            try 
            {
                testParser.OpenFile(FilePath);
                TestInfo = $"File loaded succesfully. Tests found: {testParser.GetTestCount()}";
            }
            catch (Exception e)
            {
                TestInfo = e.InnerException != null ? e.InnerException.Message : e.Message;
            }
        }

        [RelayCommand]
        private void CreateTests()
        {

        }
    }
}
