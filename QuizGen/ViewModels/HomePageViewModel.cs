using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.Core;
using PropertyModels.Extensions;
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

        private readonly List<string> letters;
        private Random _random = null!;

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
        private int longTestCreationMode;

        [ObservableProperty]
        private string? overallInfo;

        private string? selectedTestParserName;

        private IEnumerable<ITestParser> testParsers;

        [ObservableProperty]
        private ITestParser? testParser;

        public TopLevel? TopLevel;

        public ObservableCollection<string> TestParsersNames { get; set; } = [];

        public string? SelectedTestParserName
        {
            get => selectedTestParserName;
            set
            {
                SetProperty(ref selectedTestParserName, value);
                TestParser = testParsers.FirstOrDefault(p => p.GetType().Name == value);
            }
        }

        public HomePageViewModel()
        {
            letters = Enumerable.Range('A', 26).Select(i => ((char)i).ToString() + ". ").ToList();
            TelegramClient.OnShowKeyboardEvent += OnTelegramShowKeyboard;
            LoadPlugins();
        }

        private void LoadPlugins()
        {
            testParsers = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/QuizGen/Plugins")
                .Select(PluginLoader.LoadPlugin)
                .SelectMany(PluginLoader.CreateTestParsers)
                .ToList();

            foreach (var parser in testParsers)
            {
                TestParsersNames.Add(parser.GetType().Name);
            }
            if (testParsers.Any()) SelectedTestParserName = TestParsersNames.First();
        }

        [RelayCommand]
        private void CheckTests()
        {
            try
            {
                TestParser.OpenFile();
                OverallInfo += $"File loaded succesfully. Tests found: {TestParser.GetTestCount()}\n";
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
            if (TestName.IsNullOrEmpty())
            {
                OverallInfo += "Test Title cannot be empty!\n";
                return;
            }

            OverallInfo += "If test creation does not start, try to send any message to quizbot.\n";

            if (TestRangeType == 0)
            {
                var testCount = TestParser.GetTestCount();

                if (TestGrouping == 0)
                {
                    await CreateQuiz(
                        TestName
                        .Replace(from, 1.ToString())
                        .Replace(to, testCount.ToString()),
                        TestParser.GetAllTests());
                }
                else
                {
                    for (int i = 0; i < Math.Ceiling((double)testCount / TestGrouping); i++)
                    {
                        await CreateQuiz(
                            TestName
                            .Replace(from, (i * TestGrouping + 1).ToString())
                            .Replace(to, ((i + 1) * TestGrouping > testCount ? testCount : (i + 1) * TestGrouping).ToString()),
                            TestParser.GetAllTests().Skip(i * TestGrouping).Take(TestGrouping));

                        await Task.Delay(2000);
                    }
                }
            }
            else
            {
                var tests = TestParser.GetAllTests().Skip(TestRangeFrom - 1).Take(TestRangeTo - TestRangeFrom + 1);

                if (TestGrouping == 0)
                {
                    await CreateQuiz(
                        TestName.Replace(from, TestRangeFrom.ToString()).
                        Replace(to, TestRangeTo.ToString()),
                        tests);
                }
                else
                {
                    for (int i = 0; i < Math.Ceiling((double)tests.Count() / TestGrouping); i++)
                    {
                        await CreateQuiz(
                            TestName
                            .Replace(from, (TestRangeFrom + i * TestGrouping).ToString())
                            .Replace(to, (TestRangeFrom + (i + 1) * TestGrouping - 1 > TestRangeTo ? TestRangeTo : (TestRangeFrom + (i + 1) * TestGrouping - 1)).ToString()),
                            tests.Skip(i * TestGrouping).Take(TestGrouping));

                        await Task.Delay(2000);
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
                if (test.Question.Length > 255 || test.CorrectAnswer.Length > 100 || test.OtherAnswers.Any(a => a.Length > 100))
                {
                    if (LongTestCreationMode == 0) // send message first
                    {
                        _random = new Random(TestParser?.GetTestCount() ?? 49);

                        var lettersPiece = letters.Take(test.OtherAnswers.Count() + 1).ToList();
                        var answerLetter = lettersPiece.ElementAt(_random.Next(lettersPiece.Count()));
                        lettersPiece.Remove(answerLetter);

                        var otherAnswers = test.OtherAnswers.OrderBy(_ => _random.Next()).ToList();

                        var answers = new List<string>();

                        for (int i = 0; i < lettersPiece.Count(); i++)
                        {
                            answers.Add(lettersPiece[i] + otherAnswers[i]);
                        }

                        answers.Add(answerLetter + test.CorrectAnswer);
                        answers = answers.OrderBy(a => a[0]).ToList();

                        if(test.Question.Length > 255)
                        {
                            var message = test.Question + string.Join('\n', answers);

                            Test editedTest = new Test()
                            {
                                Question = "Variants:",
                                CorrectAnswer = answerLetter,
                                OtherAnswers = lettersPiece
                            };

                            await TelegramClient.SendMessage(quizbotId, message);
                            await TelegramClient.SendPoll(quizbotId, editedTest);
                        }
                        else
                        {
                            var message = string.Join('\n', answers);

                            Test editedTest = new Test()
                            {
                                Question = test.Question,
                                CorrectAnswer = answerLetter,
                                OtherAnswers = lettersPiece
                            };

                            await TelegramClient.SendMessage(quizbotId, message);
                            await TelegramClient.SendPoll(quizbotId, editedTest);
                        }
                    }
                    else if (LongTestCreationMode == 1) // trim test question/answers
                    {
                        Test trimmedTest = new Test()
                        {
                            Question = test.Question?.Length > 255 ? test.Question.Substring(0, 252) + "..." : test.Question,
                            CorrectAnswer = test.CorrectAnswer?.Length > 100 ? test.CorrectAnswer.Substring(0, 97) + "..." : test.CorrectAnswer,
                            OtherAnswers = test.OtherAnswers?.Select(a => a?.Length > 100 ? a.Substring(0, 97) + "..." : a)
                        };

                        await TelegramClient.SendPoll(quizbotId, trimmedTest);
                    }
                }
                else
                {
                    await TelegramClient.SendPoll(quizbotId, test);
                }
            }

            await TelegramClient.SendMessage(quizbotId, "/done");
        }

        private async void OnTelegramShowKeyboard(long chatId, List<KeyboardButton> keyboard)
        {
            if (chatId == quizbotId)
            {
                if (keyboard.Count == 4)
                {
                    await TelegramClient.SendMessage(quizbotId, keyboard[TestShufflingIndex].Text);
                }
                else if (keyboard.Count == 9)
                {
                    await TelegramClient.SendMessage(quizbotId, keyboard[TestDurationIndex].Text);
                }
            }
        }
    }
}
