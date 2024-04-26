using System.Collections.Generic;
using System.Text;

namespace Fallout4Downgrader
{
    public static class Tokenizer
    {
        public static IReadOnlyList<Token> Tokenize(string input)
        {
            var index = 0;
            var column = 0;
            var line = 0;
            var tokens = new List<Token>();

            void Token(TokenType type, string value)
            {
                tokens.Add(new Tokenizer.Token
                {
                    Type = type,
                    Value = value,
                    Line = line + 1,
                    Column = column,
                    Index = index
                });
            }

            while (index < input.Length)
            {
                var token = input[index];
                switch (token)
                {
                    case '{':
                        Token(TokenType.OpenCurly, token.ToString());
                        break;

                    case '}':
                        Token(TokenType.CloseCurly, token.ToString());
                        break;

                    case ' ':
                    case '\t':
                    case '\r':
                        Token(TokenType.Whitespace, token.ToString());
                        break;

                    case '\n':
                        Token(TokenType.NewLine, token.ToString());
                        column = 0;
                        line++;
                        index++;
                        continue;

                    case '"':
                        var sb = new StringBuilder();
                        while (index + 1 < input.Length && input[++index] != '"')
                        {
                            ++column;
                            sb.Append(input[index]);
                        }
                        Token(TokenType.String, sb.ToString());
                        break;
                }

                column++;
                index++;
            }

            return tokens;
        }

        public struct Token
        {
            public TokenType Type;
            public string Value;
            public int Line;
            public int Column;
            public int Index;

            public override string ToString()
            {
                return $"Type: {Type}, Value: {Value}, Line: {Line}, Column: {Column}, Index: {Index}";
            }
        }

        public enum TokenType
        {
            OpenCurly,
            CloseCurly,
            Whitespace,
            NewLine,
            String
        }
    }
}
