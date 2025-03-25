using NanoXLSX;
using PropertyModels.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestParser;

namespace BaseTestParsers
{
    public class XLSXTestParser : ITestParser
    {
        private Worksheet? worksheet;

        private int firstRow;
        private int lastRow;
        private string column = string.Empty;

        private readonly Regex letterRegex = new Regex(@"\D+");

        [DisplayName("File path:")]
        [PathBrowsable(Filters = "Excel Files(*.xlsx)|*.xlsx")]
        public string? FilePath { get; set; }

        [Browsable(false)]
        public string Description => "Parser for Excel tests. Supports format .xlsx";

        [DisplayName("Answer variants count:")]
        [Range(2, 10)]
        public int AnswerVariantsCount { get; set; } = 4;

        [DisplayName("First question cell:")]
        public string FirstQuestionCell { get; set; } = "B4";

        [DisplayName("Last question cell:")]
        public string LastQuestionCell { get; set; } = "B9";

        [DisplayName("Sheet name:")]
        public string? SheetName { get; set; }

        public IEnumerable<Test> GetAllTests()
        {
            int count = GetTestCount();
            int oneTestcellCount = AnswerVariantsCount + 1;
            var cells = worksheet.GetColumn(column).Skip(firstRow - 1).Take((lastRow - firstRow + AnswerVariantsCount) + 1);


            for (int i = 0; i < count; i++)
            {
                yield return new Test()
                {
                    Question = cells.ElementAt(i * oneTestcellCount).Value.ToString(),
                    CorrectAnswer = cells.ElementAt(i * oneTestcellCount + 1).Value.ToString(),
                    OtherAnswers = cells.Skip(i * oneTestcellCount + 2).Take(AnswerVariantsCount - 1).Select(c => c.Value.ToString())
                };
            }
        }

        public int GetTestCount()
        {
            if ((lastRow - firstRow) % (AnswerVariantsCount + 1) != 0)
                throw new ApplicationException("Invalid test count. Check your file");

            return (lastRow - firstRow) / (AnswerVariantsCount + 1);
        }

        public void OpenFile()
        {
            if(SheetName != null)
            {
                worksheet = Workbook.Load(FilePath).GetWorksheet(SheetName);
            }
            else
            {
                worksheet = Workbook.Load(FilePath).CurrentWorksheet;
            }

            calculateCells();
        }

        private void calculateCells()
        {
            column = letterRegex.Match(FirstQuestionCell).Value;
            if (column != letterRegex.Match(LastQuestionCell).Value)
            {
                throw new ApplicationException("First question cell and last question cell must be in the same column!");
            }
            firstRow = int.Parse(letterRegex.Replace(FirstQuestionCell, string.Empty));
            lastRow = int.Parse(letterRegex.Replace(LastQuestionCell, string.Empty));
        }
    }
}
