using System;
using System.Reflection;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var plugins = new string[] { "Demo", "Barcode", "Gain", "TensorFlow", "Weather" };
            foreach (var plugin in plugins)
            {
                try
                {
                    var path = System.Reflection.Assembly.GetEntryAssembly().Location;
                    Assembly ass = Assembly.LoadFrom(path.Substring(0, path.IndexOf(@"\Test\")) + @"\" + plugin + @"\bin\Debug\net9.0\" + plugin + ".dll");
                    var ins = ass.CreateInstance("Plugins.Main", true);
                    Console.WriteLine("Instantiated " + plugin + ".dll");
                }
                catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            Console.ReadLine();

        }
    }
}
