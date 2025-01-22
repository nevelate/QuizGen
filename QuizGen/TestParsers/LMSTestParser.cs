using NanoXLSX;
using QuizGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Td.Api;

namespace QuizGen.TestParsers
{
    internal class LMSTestParser : ITestParser
    {
        private Workbook workBook = null!;

        public IEnumerable<Test> GetAllTests()
        {
            var count = workBook.CurrentWorksheet.GetLastDataRowNumber();
            var cells = workBook.CurrentWorksheet.GetColumn("A").Skip(1).Take(count);

            for (int i = 0; i < count / 5; i++)
            {
                yield return new Test()
                {
                    Question = new FormattedText(cells.ElementAt(i * 5).Value.ToString(), null),
                    Answers = cells.Skip(i * 5 + 1).Take(4).Select(c => new FormattedText(c.Value.ToString(), null))
                };
            }
        }

        public int GetTestCount()
        {
            return workBook.CurrentWorksheet.GetLastDataRowNumber() / 5;
        }

        public void OpenFile(string path)
        {
            workBook = Workbook.Load(path);
        }
    }
}
