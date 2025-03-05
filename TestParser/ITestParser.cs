using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TestParser
{
    public interface ITestParser
    {
        string? FilePath { get; set; }
        string Description { get; }

        void OpenFile();

        int GetTestCount();

        IEnumerable<Test> GetAllTests();
    }
}
