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
}
