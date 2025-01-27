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
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(ITestParser).IsAssignableFrom(type))
                {
                    ITestParser result = Activator.CreateInstance(type) as ITestParser;
                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }

            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ITestParser in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }
        }
    }
}
