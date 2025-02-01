using Avalonia.Media.TextFormatting.Unicode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TestParser;

namespace QuizGen
{
    internal static class PluginLoader
    {
        public static Assembly LoadPlugin(string path)
        {
            string pluginLocation = Path.GetFullPath(path.Replace('\\', Path.DirectorySeparatorChar));
            PluginLoadContext loadContext = new PluginLoadContext(pluginLocation);
            return loadContext.LoadFromAssemblyName(AssemblyName.GetAssemblyName(pluginLocation));
        }

        public static IEnumerable<ITestParser> CreateTestParsers(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(ITestParser).IsAssignableFrom(type))
                {
                    ITestParser? result = Activator.CreateInstance(type) as ITestParser;
                    if (result != null)
                    {
                        yield return result;
                    }
                }
            }
        }
    }
}
