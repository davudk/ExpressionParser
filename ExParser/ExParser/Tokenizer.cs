using System.Linq;

namespace ExParser {
    class Tokenizer : ITokenReader {
        static readonly string OperatorChars = "(),+-*/^";
        string s;
        int i;
        Token peekedToken;
        public string Text => s;

        public Tokenizer(string s) {
            this.s = s;
        }

        public Token GetToken() {
            if (peekedToken != null) { // return peeked token, if possible
                Token tok = peekedToken;
                peekedToken = null;
                return tok;
            }
            
            while (!EOF()) { // skip all the white space
                if (char.IsWhiteSpace(s, i)) i++;
                else break;
            }
            if (EOF()) return null; // return null if EOF

            char c = s[i];
            switch (c) { // check if the token is a single-char operator
                case '(': i++; return Token.LParen;
                case ')': i++; return Token.RParen;
                case ',': i++; return Token.Comma;
                case '+': i++; return Token.AddOp;
                case '-': i++; return Token.SubOp;
                case '*': i++; return Token.MultOp;
                case '/': i++; return Token.DivOp;
                case '^': i++; return Token.PowOp;
            }

            // at this point, the token is either a number or a label
            if (s[i] == '.' || char.IsDigit(s, i)) { // is it a number?
                int start = i;
                do {
                    i++;
                } while (!EOF() && (s[i] == '.' || char.IsDigit(s, i)));
                int len = i - start;
                return new Token(s.Substring(start, len), TokenType.Number);
                // Number ::= [0-9.]*
            } else { // whatever the char is, it's a label; don't check if it's a letter
                int start = i;
                do {
                    i++;
                } while (!EOF() && !OperatorChars.Contains(s[i])); // check that it's not an operator
                int len = i - start;
                return new Token(s.Substring(start, len), TokenType.Label);
                // Label ::= [^0-9\(\)\+-\*/\^][^\(\)\+-\*/\^]*
            }
        }

        public Token PeekToken() {
            if (peekedToken == null) return peekedToken = GetToken();
            else return peekedToken;
        }

        bool EOF() => i >= s.Length;
    }
}
