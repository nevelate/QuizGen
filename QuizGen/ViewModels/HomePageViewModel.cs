using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NanoXLSX.Exceptions;
using QuizGen.Models;
using QuizGen.TestParsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;

namespace QuizGen.ViewModels
{
    public partial class HomePageViewModel : ViewModelBase
    {
        private const long quizbotId = 983000232;

        [ObservableProperty]
        private string? filePath;
        [ObservableProperty]
        private string? testName;
        [ObservableProperty]
        private string? testDescription;

        [ObservableProperty]
        private int testRangeType;
        [ObservableProperty]
        private int testRangeFrom;
        [ObservableProperty]
        private int testRangeTo;
        [ObservableProperty]
        private int testGrouping;

        [ObservableProperty]
        private int testDurationIndex;
        [ObservableProperty]
        private int testShufflingIndex;

        [ObservableProperty]
        private string? overallInfo;

        private ITestParser testParser = null!;
        public TopLevel? TopLevel;

        public HomePageViewModel()
        {
            TelegramClient.OnShowKeyboardEvent += OnTelegramShowKeyboard;
        }

        [RelayCommand]
        private async Task OpenFile()
        {
            if (TopLevel != null)
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
                OverallInfo += $"File loaded succesfully. Tests found: {testParser.GetTestCount()}\n";
            }
            catch (Exception e)
            {
                OverallInfo += e.InnerException != null ? e.InnerException.Message : e.Message;
                OverallInfo += "\n";
            }
        }

        [RelayCommand]
        private void Create()
        {
            if (TestRangeType == 0)
            {
                if (TestGrouping == 0)
                {
                    InitializeQuiz(string.Format(TestName, $"1 - {testParser.GetTestCount()}"));

                    foreach (var test in testParser.GetAllTests())
                    {
                        TelegramClient.SendPoll(quizbotId, test);
                    }

                    TelegramClient.SendMessage(quizbotId, "/done");
                }
                else
                {

                }
            }
            else
            {

            }
        }

        [RelayCommand]
        private void Clear()
        {

        }

        private void InitializeQuiz(string name)
        {
            TelegramClient.SendMessage(quizbotId, "/cancel");
            TelegramClient.SendMessage(quizbotId, "/stop");

            TelegramClient.SendMessage(quizbotId, "/newquiz");

            if (string.IsNullOrEmpty(name))
            {
                OverallInfo += "Test name cannot be empty!\n";
                return;
            }
            TelegramClient.SendMessage(quizbotId, TestName);
            TelegramClient.SendMessage(quizbotId, string.IsNullOrEmpty(TestDescription) ? "/skip" : TestDescription);
        }

        private void CreateQuiz(string name, IEnumerable<Test> tests)
        {
            InitializeQuiz(name);

            foreach (var test in tests)
            {
                TelegramClient.SendPoll(quizbotId, test);
            }

            TelegramClient.SendMessage(quizbotId, "/done");
        }

        private void OnTelegramShowKeyboard(long chatId, List<KeyboardButton> keyboard)
        {
            if(chatId == quizbotId)
            {
                if(keyboard.Count == 4)
                {
                    TelegramClient.SendMessage(quizbotId, keyboard[TestShufflingIndex].Text);
                }
                else
                {
                    TelegramClient.SendMessage(quizbotId, keyboard[TestDurationIndex].Text);
                }
            }
        }
    }
}
