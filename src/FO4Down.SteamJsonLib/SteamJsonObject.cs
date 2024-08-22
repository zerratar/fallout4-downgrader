

using System.Text;

namespace FO4Down.SteamJson
{
    public class SteamJsonObject
    {
        private Dictionary<string, string> properties = new Dictionary<string, string>();
        private List<string> strings = new List<string>();

        public string this[string key]
        {
            get => properties[key];
            set => properties[key] = value;
        }

        public IReadOnlyList<string> Strings => strings;
        public IReadOnlyDictionary<string, string> Properties => properties;
        public string Identifier { get; set; }
        public List<SteamJsonObject> Children { get; set; } = new List<SteamJsonObject>();
        public SteamJsonObject Parent { get; set; }

        public void MergeWithParent()
        {
            if (this.Parent == null)
                return;

            if (this.Identifier == null)
            {
                for (var i = 0; i < this.strings.Count; ++i)
                {
                    var s = this.strings[i];
                    this.Parent.AddString(s);
                }

                strings.Clear();
                properties.Clear();

                foreach (var c in this.Children)
                {
                    c.Parent = this.Parent;
                    this.Parent.Children.Add(c);
                }
            }
        }

        public void AddString(string value)
        {
            strings.Add(value);

            // rebuild values dictionary if strings count is % 2 == 0
            if (strings.Count % 2 == 0)
            {
                properties.Clear();
                for (var i = 0; i < strings.Count; i += 2)
                {
                    properties[strings[i]] = strings[i + 1];
                }
            }
        }

        public override string ToString()
        {
            return Identifier + ": (Properties: " + properties.Count + ", Children: " + Children.Count + ")";
        }

        public string AsString()
        {
            // build the string
            var sb = new StringBuilder();
            VisitThis(sb, 0);
            return sb.ToString();
        }

        private void VisitThis(StringBuilder sb, int indent)
        {
            var childIndent = indent + 1;
            var objIndent = new string('\t', indent);
            var propIndent = new string('\t', childIndent);
            sb.AppendLine(objIndent + "\"" + Identifier + "\"");
            sb.AppendLine(objIndent + "{");

            foreach (var prop in properties)
            {
                sb.AppendLine(propIndent + "\"" + prop.Key + "\"\t\t\"" + prop.Value + "\"");
            }

            foreach (var child in Children)
            {
                child.VisitThis(sb, childIndent);
            }

            sb.AppendLine(objIndent + "}");
        }

        public SteamJsonObject? GetChild(string v)
        {
            return this.Children.FirstOrDefault(x => x.Identifier == v);
        }
    }
}
