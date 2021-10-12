using PluginShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Plugins
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
            json = json.Replace("VERSION", "v" + typeof(Utils).Assembly.GetName().Version);
            return json;

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
