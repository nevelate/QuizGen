using PropertyModels.ComponentModel.DataAnnotations;
using PropertyModels.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestParser;
using Wordroller;

namespace BaseTestParsers
{
    public class HEMISTestParser : ITestParser
    {
        [DisplayName("File path:")]
        [PathBrowsable(Filters = "HEMIS Files(*.txt;*.docx)|*.txt;*.docx")]
        public string? FilePath { get; set; }

        [Browsable(false)]
        public string Description => "Parser for tests that are used in final tests in universities using HEMIS. Supports formats .txt, .docx";

        private bool isDocx;
        private string? data;

        private static readonly Regex questionRegex = new Regex(@"\++[^=]+");
        private static readonly Regex correctAnswerRegex = new Regex(@"\=+\#[^=]+");
        private static readonly Regex answerRegex = new Regex(@"\=+[^=#+]+");

        public IEnumerable<Test> GetAllTests()
        {
            int count = GetTestCount();
            int start = questionRegex.Match(data).Index;
            int end = 0;
            string test = string.Empty;

            for (int i = 0; i < count; i++)
            {
                end = questionRegex.Match(data, start + 5).Index;

                if (end == 0)
                {
                    end = data.Length;
                }

                test = data.Substring(start, end - start);
                start = end;

                yield return new Test()
                {
                    Question = questionRegex.Match(test).Value.Replace("+++++", "").Replace("++++", ""),
                    CorrectAnswer = correctAnswerRegex.Match(test).Value.Replace("=====# ", ""),
                    OtherAnswers = answerRegex.Matches(test).Select(m => m.Value.Replace("=====", ""))
                };
            }
        }

        public int GetTestCount()
        {
            return Regex.Matches(data, questionRegex.ToString()).Count;
        }

        public void OpenFile()
        {
            FileInfo fileInfo = new FileInfo(FilePath);
            isDocx = fileInfo.Extension switch
            {
                ".txt" => false,
                ".docx" => true,
                _ => throw new ApplicationException("Invalid file extension")
            };

            if (isDocx)
            {
                using var document = new WordDocument(File.OpenRead(FilePath));
                data = document.Body.Xml.Value;
            }
            else
            {
                data = File.ReadAllText(FilePath);
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ApplicationException("File is empty");
            }
        }
    }
}
