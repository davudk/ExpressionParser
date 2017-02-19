using System;
using System.Collections.Generic;

namespace ExParser {
    public class Instance {
        public Dictionary<string, Func<double[], double>> Functions { get; private set; }
        public Dictionary<string, double> Constants { get; private set; }

        public Instance() {
            Functions = new Dictionary<string, Func<double[], double>>();
            Constants = new Dictionary<string, double>();
        }

        public double Evaluate(string s) {
            Tokenizer tknr = new Tokenizer(s);

            LinkedList<double> numbers = new LinkedList<double>();
            LinkedList<TokenType> operators = new LinkedList<TokenType>();

            Token tok;
            while ((tok = tknr.GetToken()) != null) {
                switch (tok.Type) {
                case TokenType.Number: {
                        double value;
                        if (double.TryParse(tok.Lexeme, out value)) {
                            numbers.AddLast(value);
                        } else {
                            throw new NotImplementedException("Unable to parse double.");
                        }
                    }
                    break;
                case TokenType.AddOp:
                case TokenType.SubOp:
                case TokenType.MultOp:
                case TokenType.DivOp:
                case TokenType.PowOp:
                    operators.AddLast(tok.Type);
                    break;
                case TokenType.Label: { // either a constant or a function call
                        string name = tok.Lexeme;
                        double value;
                        if ((tok = tknr.PeekToken()) != null && tok.Type == TokenType.LParen) { // function call

                        } else if (Constants.TryGetValue(name, out value)) { // constant
                            numbers.AddLast(value);
                        } else { // constant, but it doesn't exist in the constant map
                            throw new NotImplementedException("Constant does not exist.");
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("Unexpected token type: " + tok.Type.ToString());
                }
            }

            // the amount of operators must be one-less than the amount of numbers
            if (operators.Count == numbers.Count - 1) {

                return 0; // evaluating soon

            } else {
                throw new NotImplementedException("Operator/number mismatch.");
            }
        }
    }
}
