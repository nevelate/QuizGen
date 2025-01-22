using QuizGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizGen.TestParsers
{
    internal interface ITestParser
    {
        void OpenFile(string path);

        int GetTestCount();

        IEnumerable<Test> GetAllTests();
    }
}
