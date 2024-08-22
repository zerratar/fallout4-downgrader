﻿

namespace FO4Down.SteamJson
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
                var node = Parse(token, tokens, ref index);

                AddNode(node);

                ++index;
            }

            return Compress(BuildObject(nodes));
        }

        private static SteamJsonObject Compress(SteamJsonObject obj)
        {
            // search down the tree for children without identifier, then merge it with their parent
            // do this recursively

            var children = obj.Children.ToList();

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];
                children[i] = Compress(child);
            }

            if (obj.Identifier == null)
            {
                obj.MergeWithParent();
            }

            // remove empty children
            obj.Children.RemoveAll(x => x.Identifier == null);

            return obj;
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
            var firstToken = token;
            // skip whitespaces and newlines
            while (token.Type != Tokenizer.TokenType.String
                && token.Type != Tokenizer.TokenType.CloseCurly
                && index + 1 < tokens.Count)
            {
                token = tokens[++index];
            }

            while (token.Type != Tokenizer.TokenType.CloseCurly)
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

            }
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
    }
}
