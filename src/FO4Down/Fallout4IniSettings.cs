using System.Globalization;
using System.Text;

namespace FO4Down
{
    public class Fallout4IniSettings
    {
        // Additional sections similar to the above

        private Dictionary<string, Section> sections = new Dictionary<string, Section>();

        public Section this[string section]
        {
            get => sections[section];
            set => sections[section] = value;
        }

        public string Path { get; set; }

        public void Save()
        {
            if (System.IO.Path.Exists(Path))
            {
                // make a backup

                var backupPath = System.IO.Path.Combine(Path, "_" + DateTime.Now.Ticks + ".backup");
                File.Copy(Path, backupPath, true);
            }

            var sb = new StringBuilder();
            foreach (var section in sections)
            {
                sb.AppendLine("[" + section.Key + "]");
                foreach (var value in section.Value.Properties)
                {
                    sb.AppendLine(value.Key + "=" + value.Key);
                }

                sb.AppendLine();
            }

            File.WriteAllText(Path, sb.ToString());
        }

        public static Fallout4IniSettings FromIni(string filePath)
        {
            var iniSettings = new Fallout4IniSettings();
            iniSettings.Path = filePath;
            var iniLines = File.ReadAllLines(filePath);
            string currentSectionName = null;
            Section currentSection = null;

            foreach (var line in iniLines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSectionName = line.Trim('[', ']');
                    currentSection = new Section();
                    currentSection.Name = currentSectionName;
                    iniSettings.sections[currentSection.Name] = currentSection;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith(";") && currentSection != null)
                {
                    var splitIndex = line.IndexOf('=');
                    if (splitIndex != -1)
                    {
                        var key = line.Substring(0, splitIndex).Trim(); // remove white spaces in name
                        currentSection.Properties[key] = line.Substring(splitIndex + 1).Trim();
                    }
                }
            }

            return iniSettings;
        }

        public class Section
        {
            public string Name;
            public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

            public string this[string key]
            {
                get
                {
                    Properties.TryGetValue(key, out var result);
                    return result;
                }

                set
                {
                    if (!Properties.ContainsKey(key))
                    {
                        return;
                    }

                    Properties[key] = value;
                }
            }


            public T GetValue<T>(string key)
            {
                var value = this[key];
                var type = typeof(T);
                object result = null;

                if (type == typeof(bool))
                {
                    if (bool.TryParse(value, out var r))
                    {
                        result = r;
                    }
                    else
                    {
                        result = value == "1";
                    }
                }
                else if (type == typeof(string))
                {
                    result = value;
                }
                else if (type == typeof(double))
                {
                    if (double.TryParse(value, NumberStyles.AllowDecimalPoint, null, out var doubleValue) ||
                        double.TryParse(value.Replace('.', ','), NumberStyles.AllowDecimalPoint, null, out doubleValue) ||
                        double.TryParse(value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, null, out doubleValue))
                        result = doubleValue;
                }
                else if (type == typeof(float))
                {
                    if (value.EndsWith("F"))
                    {
                        value = value.Remove(value.Length - 1);
                    }

                    if (float.TryParse(value, NumberStyles.AllowDecimalPoint, null, out var v) ||
                        float.TryParse(value.Replace('.', ','), NumberStyles.AllowDecimalPoint, null, out v) ||
                        float.TryParse(value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, null, out v))
                        result = v;
                }
                else
                {
                    result = Convert.ChangeType(value, type);
                }

                return (T)result;
            }

        }
    }
}
