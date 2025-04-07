using System;
using WixSharp;

namespace WixSharpSetup
{
    public class Program
    {
        static void Main()
        {
            var project = new Project("MyProduct",
                              new Dir(@"%ProgramFiles%\My Company\My Product",
                                  new File("Program.cs")));

            project.GUID = new Guid("9eec5927-823a-4695-96a8-7d66eec07140");
            //project.SourceBaseDir = "<input dir path>";
            //project.OutDir = "<output dir path>";

            project.BuildMsi();
        }
    }
}