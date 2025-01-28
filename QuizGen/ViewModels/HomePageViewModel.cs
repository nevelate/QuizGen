using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Td.Api;
using TestParser;

namespace QuizGen.ViewModels
{
    public partial class HomePageViewModel : ViewModelBase
    {
        private const long quizbotId = 983000232;
        private const string from = "{from}";
        private const string to = "{to}";

        [ObservableProperty]
        private string? filePath;
        [ObservableProperty]
        private string? testName;
        [ObservableProperty]
        private string? testDescription;

        [ObservableProperty]
        private int testRangeType;
        [ObservableProperty]
        private int testRangeFrom = 1;
        [ObservableProperty]
        private int testRangeTo = 1;
        [ObservableProperty]
        private int testGrouping;

        [ObservableProperty]
        private int testDurationIndex;
        [ObservableProperty]
        private int testShufflingIndex;

        [ObservableProperty]
        private string? overallInfo;

        [ObservableProperty]
        private string? testParserName;

        private IEnumerable<ITestParser> testParsers;
        private ITestParser testParser = null!;

        public TopLevel? TopLevel;

        public ObservableCollection<string> TestParsersNames { get; set; } = [];

        public HomePageViewModel()
        {
            TelegramClient.OnShowKeyboardEvent += OnTelegramShowKeyboard;
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            testParsers = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Plugins")
                .Select(PluginLoader.LoadPlugin)
                .SelectMany(PluginLoader.CreateTestParsers)
                .ToList();

            foreach (var parser in testParsers)
            {
                TestParsersNames.Add(parser.GetType().Name);
            }

            TestParserName = TestParsersNames.First();
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
            testParser = testParsers.First(p => p.GetType().Name == TestParserName);

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
        private async Task Create()
        {
            if (TestRangeType == 0)
            {
                if (TestGrouping == 0)
                {
                    await CreateQuiz(
                        TestName
                        .Replace(from, 1.ToString())
                        .Replace(to, testParser.GetTestCount().ToString()),
                        testParser.GetAllTests());
                }
                else
                {
                    for (int i = 0; i < testParser.GetTestCount() / TestGrouping; i++)
                    {
                        await CreateQuiz(
                            TestName
                            .Replace(from, (i * TestGrouping + 1).ToString())
                            .Replace(to, ((i + 1) * TestGrouping).ToString()),
                            testParser.GetAllTests().Skip(i * TestGrouping).Take(TestGrouping));

                        await Task.Delay(3000);
                    }
                }
            }
            else
            {
                var tests = testParser.GetAllTests().Skip(TestRangeFrom - 1).Take(TestRangeTo - TestRangeFrom + 1);

                if (TestGrouping == 0)
                {
                    await CreateQuiz(
                        TestName.Replace(from, TestRangeFrom.ToString()).
                        Replace(to, TestRangeTo.ToString()),
                        tests);
                }
                else
                {
                    for (int i = 0; i < tests.Count() / TestGrouping; i++)
                    {
                        await CreateQuiz(
                            TestName
                            .Replace(from, (TestRangeFrom + i * TestGrouping + 1).ToString())
                            .Replace(to, (TestRangeFrom + (i + 1) * TestGrouping).ToString()),
                            tests.Skip(i * TestGrouping).Take(TestGrouping));

                        await Task.Delay(3000);
                    }
                }
            }
        }

        [RelayCommand]
        private void Clear()
        {
            TestName = null;
            TestDescription = null;
            TestRangeType = 0;
            TestRangeFrom = 0;
            TestRangeTo = 0;
            TestDurationIndex = 0;
            TestShufflingIndex = 0;
        }

        private async Task InitializeQuiz(string name)
        {
            await TelegramClient.SendMessage(quizbotId, "/cancel");
            await TelegramClient.SendMessage(quizbotId, "/stop");

            await TelegramClient.SendMessage(quizbotId, "/newquiz");
            
            await TelegramClient.SendMessage(quizbotId, name);
            await TelegramClient.SendMessage(quizbotId, string.IsNullOrEmpty(TestDescription) ? "/skip" : TestDescription);
        }

        private async Task CreateQuiz(string name, IEnumerable<Test> tests)
        {
            if (string.IsNullOrEmpty(name))
            {
                OverallInfo += "Test name cannot be empty!\n";
                return;
            }

            await InitializeQuiz(name);

            foreach (var test in tests)
            {
                await TelegramClient.SendPoll(quizbotId, test);
            }

            await TelegramClient.SendMessage(quizbotId, "/done");
        }

        private void OnTelegramShowKeyboard(long chatId, List<KeyboardButton> keyboard)
        {
            if (chatId == quizbotId)
            {
                if (keyboard.Count == 4)
                {
                    TelegramClient.SendMessage(quizbotId, keyboard[TestShufflingIndex].Text);
                }
                else if (keyboard.Count == 9)
                {
                    TelegramClient.SendMessage(quizbotId, keyboard[TestDurationIndex].Text);
                }
            }
        }
    }
}
