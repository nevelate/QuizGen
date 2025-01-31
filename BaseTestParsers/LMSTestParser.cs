using NanoXLSX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestParser;

namespace BaseTestParsers
{
    public class LMSTestParser : ITestParser
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
                    Question = cells.ElementAt(i * 5).Value.ToString(),
                    CorrectAnswer = cells.ElementAt(i * 5 + 1).Value.ToString(),
                    OtherAnswers = cells.Skip(i * 5 + 2).Take(3).Select(c => c.Value.ToString())
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
