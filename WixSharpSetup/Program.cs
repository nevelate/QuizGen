using System;
using WixSharp;

namespace WixSharpSetup
{
    public class Program
    {
        static void Main()
        {
            var project = new Project("QuizGen",
                              new Dir(@"%ProgramFiles%\QuizGen",
                                  new Files(@"..\QuizGen\bin\Release\net8.0\publish\win-x64\*.*", f => !f.EndsWith(".pdb"))),
                              new Dir(@"%AppData%\QuizGen\Plugins",
                              new Files(@"..\BaseTestParsers\bin\Release\net8.0\*.*", f => f.EndsWith(".dll"))));

            project.GUID = new Guid("9eec5927-823a-4695-96a8-7d66eec07140");

            project.BuildMsi();
        }
    }
}