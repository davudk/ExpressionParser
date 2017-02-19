using System.Collections.Generic;

namespace ExParser {
    class TokenBuffer : ITokenReader {
        public int Index { get; private set; }
        Token[] tokens;

        public TokenBuffer(List<Token> tokens) : this(tokens.ToArray()) { }
        public TokenBuffer(Token[] tokens) {
            this.tokens = tokens;
        }

        public Token GetToken() {
            return EOF() ? null : tokens[Index++];
        }

        public Token PeekToken() {
            return EOF() ? null : tokens[Index];
        }

        bool EOF() => Index >= tokens.Length;
    }
}
