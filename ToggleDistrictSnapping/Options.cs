using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Debug = UnityEngine.Debug;

namespace ToggleDistrictSnapping
{
    public struct Options
    {
        public bool defaultToNoSnapping;
    }

    public static class OptionsLoader
    {
        private const string fileName = "ToggleDistrictSnapping-Options.xml";

        public static void LoadOptions()
        {
            ModInfo.Options = ModOptions.None;
            Options options;

            // read the options in from a file, if it exists
            try {
                var xml = new XmlSerializer(typeof(Options));
                using (var reader = new StreamReader(fileName)) {
                    options = (Options)xml.Deserialize(reader);
                }
            }
            catch (FileNotFoundException) {
                // No file, set the default options
                options = new Options {
                    defaultToNoSnapping = false
                };
                SaveOptions(options);
            }
            catch (Exception e) {
                Debug.LogError("Error loading options");
                return;
            }

            if (options.defaultToNoSnapping)
                ModInfo.Options |= ModOptions.DefaultToNoSnapping;
        }

        public static void SaveOptions()
        {
            var options = new Options();

            if ((ModInfo.Options & ModOptions.DefaultToNoSnapping) != 0)
                options.defaultToNoSnapping = true;

            SaveOptions(options);
        }

        public static void SaveOptions(Options options)
        {
            try {
                var xml = new XmlSerializer(typeof(Options));
                using (var writer = new StreamWriter(fileName)) {
                    xml.Serialize(writer, options);
                }
            }
            catch (Exception e) {
                Debug.LogError("Error saving options");
            }
        }
    }
}
