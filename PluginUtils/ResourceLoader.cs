using PluginUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PluginUtils
{
    internal class ResourceLoader
    {
        public static string ResourcePath = "Plugins.";
        public static string LoadJson(string languageCode)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var json = LoadResource(assembly, "json.config_" + languageCode + ".json");
            if (string.IsNullOrEmpty(json))
                json = LoadResource(assembly, "json.config_en.json");
            json = json.Replace("VERSION", "v" + typeof(ResourceLoader).Assembly.GetName().Version);
            return json;

        }

        public static string GetResourceText(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            filename = filename.Replace("/", ".").Replace("\\", ".");

            var resourceName = assembly.GetManifestResourceNames().First(s => s.EndsWith(filename.Replace("/", "."), StringComparison.CurrentCultureIgnoreCase));

            using (StreamReader reader = new StreamReader(assembly.GetManifestResourceStream(resourceName),
                                                          detectEncodingFromByteOrderMarks: true))
            {
                return reader.ReadToEnd();
            }
        }

        public static byte[] GetResourceBytes(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            filename = filename.Replace("/", ".").Replace("\\", ".");

            var resourceName = assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(filename.Replace("/", "."), StringComparison.CurrentCultureIgnoreCase));
            if (resourceName == null)
                return null;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    throw new InvalidOperationException("Could not load manifest resource stream.");
                }
                using (var memstream = new MemoryStream())
                {
                    stream.CopyTo(memstream);
                    return memstream.ToArray();
                }
            }
        }


        private static string LoadResource(Assembly assembly, string resourceName)
        {
            resourceName = ResourcePath + resourceName;
            if (!assembly.GetManifestResourceNames().Contains(resourceName))
                return "";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            return "";
        }
    }
}
