using System.Collections.Generic;
using System.Linq;

namespace Fallout4Downgrader
{
    public static class SteamJsonParser
    {
        public static SteamJsonObject Parse(string input)
        {
            var tokens = Tokenizer.Tokenize(input);
            var nodes = new List<Node>();
            var index = 0;

            void AddNode(Node node)
            {
                if (node != null)
                {
                    nodes.Add(node);
                }
            }

            while (index < tokens.Count)
            {
                var token = tokens[index];

                AddNode(Parse(token, tokens, ref index));

                ++index;
            }

            return BuildObject(nodes);
        }

        private static Node Parse(
            Tokenizer.Token token,
            IReadOnlyList<Tokenizer.Token> tokens,
            ref int index)
        {
            switch (token.Type)
            {
                case Tokenizer.TokenType.String:
                    return ParseIdentifier(token, tokens, ref index);
                case Tokenizer.TokenType.OpenCurly:
                    return ParseObject(token, tokens, ref index);
            }

            return null;
        }

        private static SteamJsonObject BuildObject(IReadOnlyList<Node> nodes)
        {
            var obj = new SteamJsonObject();

            // check if this is a "string" followed by an "object"
            if (nodes.Count == 2)
            {
                if (nodes[0] is StringNode identity && nodes[1] is ObjectNode objNode)
                {
                    obj.Identifier = identity.Value;
                    var child = BuildObject(objNode.Children.Cast<Node>().ToList());
                    child.Parent = obj;
                    obj.Children.Add(child);
                    return obj;
                }
            }

            for (var i = 0; i < nodes.Count; ++i)
            {
                if (nodes[i] is StringNode strNode)
                {
                    // check if next one is object node
                    if (i + 1 < nodes.Count && nodes[i + 1] is ObjectNode)
                    {
                        var child = BuildObject(new Node[] { nodes[i], nodes[i + 1] });
                        child.Parent = obj;
                        obj.Children.Add(child);
                    }
                    else
                    {
                        obj.AddString(strNode.Value);
                    }
                }
            }

            return obj;
        }

        private static Node ParseObject(
            Tokenizer.Token token,
            IReadOnlyList<Tokenizer.Token> tokens,
            ref int index)
        {
            var children = new List<Node>();

            // skip whitespaces and newlines
            while (token.Type != Tokenizer.TokenType.String 
                && token.Type != Tokenizer.TokenType.CloseCurly 
                && index + 1 < tokens.Count)
            {
                token = tokens[++index];
            }

            do
            {
                var child = Parse(token, tokens, ref index);

                if (child != null)
                {
                    children.Add(child);
                }

                if (index + 1 >= tokens.Count)
                {
                    break;
                }

                token = tokens[++index];

            } while (token.Type != Tokenizer.TokenType.CloseCurly);
            // Parse()
            return new ObjectNode
            {
                Source = token,
                Children = children
            };
        }

        private static Node ParseIdentifier(
            Tokenizer.Token token,
            IReadOnlyList<Tokenizer.Token> tokens,
            ref int index)
        {
            return new StringNode
            {
                Source = token,
                Value = token.Value.Replace("\\\\", "\\")
            };
        }

        public class Node
        {
            public Tokenizer.Token Source { get; set; }
        }

        public class ObjectNode : Node
        {
            public List<Node> Children { get; set; } = new List<Node>();

            public override string ToString()
            {
                return "ObjectNode: " + string.Join(", ", Children);
            }
        }

        public class StringNode : Node
        {
            public string Value { get; set; }

            public override string ToString()
            {
                return "StringNode: " + Value;
            }
        }

        public class SteamJsonObject
        {
            private Dictionary<string, string> _values = new Dictionary<string, string>();
            private List<string> strings = new List<string>();

            public string this[string key]
            {
                get => _values[key];
                set => _values[key] = value;
            }

            public IReadOnlyList<string> Strings => strings;

            public string Identifier { get; set; }

            public List<SteamJsonObject> Children { get; set; } = new List<SteamJsonObject>();
            public SteamJsonObject Parent { get; set; }

            public void AddString(string value)
            {
                strings.Add(value);

                // rebuild values dictionary if strings count is % 2 == 0
                if (strings.Count % 2 == 0)
                {
                    _values.Clear();
                    for (var i = 0; i < strings.Count; i += 2)
                    {
                        _values[strings[i]] = strings[i + 1];
                    }
                }
            }
        }
    }
}
