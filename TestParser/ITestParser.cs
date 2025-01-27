using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TestParser
{
    public interface ITestParser
    {
        void OpenFile(string path);

        int GetTestCount();

        IEnumerable<Test> GetAllTests();
    }
}
