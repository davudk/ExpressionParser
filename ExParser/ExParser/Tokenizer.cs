using System.Linq;

namespace ExParser {
    enum TokenType {
        Error,
        Number,
        Label,
        LParen, RParen, Comma,
        AddOp, SubOp, MultOp, DivOp, PowOp
    }
    class Token {
        public static readonly Token LParen = new Token("(", TokenType.LParen);
        public static readonly Token RParen = new Token(")", TokenType.RParen);
        public static readonly Token Comma = new Token(",", TokenType.Comma);
        public static readonly Token AddOp = new Token("+", TokenType.AddOp);
        public static readonly Token SubOp = new Token("-", TokenType.SubOp);
        public static readonly Token MultOp = new Token("*", TokenType.MultOp);
        public static readonly Token DivOp = new Token("/", TokenType.DivOp);
        public static readonly Token PowOp = new Token("^", TokenType.PowOp);
        public string Lexeme { get; private set; }
        public TokenType Type { get; private set; }

        public Token(string lexeme, TokenType type) {
            Lexeme = lexeme;
            Type = type;
        }
    }
    class Tokenizer {
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
            }
            if (EOF()) return null; // return null if EOF

            char c = s[i];
            switch (c) { // check if the token is a single-char operator
                case '(': return Token.LParen;
                case ')': return Token.RParen;
                case ',': return Token.Comma;
                case '+': return Token.AddOp;
                case '-': return Token.SubOp;
                case '*': return Token.MultOp;
                case '/': return Token.DivOp;
                case '^': return Token.PowOp;
            }

            // at this point, the token is either a number or a label
            if (char.IsDigit(s, i)) { // is it a number?
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
