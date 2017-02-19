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
            return Evaluate(new Tokenizer(s));
        }
        double Evaluate(ITokenReader tknr) {
            List<double> numbers = new List<double>();
            List<TokenType> operators = new List<TokenType>();

            Token tok;
            while ((tok = tknr.GetToken()) != null) {
                switch (tok.Type) {
                case TokenType.Number: {
                        double value;
                        if (double.TryParse(tok.Lexeme, out value)) {
                            numbers.Add(value);
                        } else {
                            throw new NotImplementedException("Unable to parse double.");
                        }
                    }
                    break;
                case TokenType.LParen: {
                        List<Token> tokens = new List<Token>();
                        int level = 1;
                        while ((tok = tknr.GetToken()) != null) { // get the tokens within the parenthesis
                            if (tok.Type == TokenType.RParen) {
                                level -= 1;
                                if (level == 0) break;
                            }
                            tokens.Add(tok); // add them to the token buffer
                        }

                        // evaluate the token buffer recursively
                        double res = Evaluate(new TokenBuffer(tokens));
                        // add result to numbers list
                        numbers.Add(res);
                    }
                    break;
                case TokenType.AddOp:
                case TokenType.SubOp:
                case TokenType.MultOp:
                case TokenType.DivOp:
                case TokenType.PowOp:
                    operators.Add(tok.Type);
                    break;
                case TokenType.Label: { // either a constant or a function call
                        string name = tok.Lexeme;
                        double value;
                        if ((tok = tknr.PeekToken()) != null && tok.Type == TokenType.LParen) { // function call

                        } else if (Constants.TryGetValue(name, out value)) { // constant
                            numbers.Add(value);
                        } else { // constant, but it doesn't exist in the constant map
                            throw new NotImplementedException("Constant does not exist.");
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException("Unexpected token type: " + tok.Type.ToString());
                }
            }

            if (operators.Count == 0 && numbers.Count == 0) {
                throw new NotImplementedException("Empty input.");
            }

            return EvaluateLists(numbers, operators);
        }

        double EvaluateLists(List<double> numbers, List<TokenType> operators) {
            // the amount of operators must be one-less than the amount of numbers

            if (operators.Count != numbers.Count - 1) {
                throw new NotImplementedException("Operator/number mismatch.");
            }

            // operator precedence can be hard coded (no table) since it doesn't change
            int index;

            // PowOp ^
            while ((index = operators.IndexOf(TokenType.PowOp)) >= 0) {
                operators.RemoveAt(index); // remove the operator
                double rhs = numbers[index + 1]; numbers.RemoveAt(index + 1); // remove the rhs number
                numbers[index] = Math.Pow(numbers[index], rhs); // calculate, and put into lhs number
            }

            // (MultOp *)  and  (DivOp /)
            while ((index = IndexOfEitherOr(operators, TokenType.MultOp, TokenType.DivOp)) >= 0) {
                TokenType op = operators[index]; operators.RemoveAt(index); // remove the operator
                double rhs = numbers[index + 1]; numbers.RemoveAt(index + 1); // remove the rhs number
                if (op == TokenType.MultOp) {
                    numbers[index] = numbers[index] * rhs; // multiply, and put into lhs number
                } else if (op == TokenType.DivOp) {
                    numbers[index] = numbers[index] / rhs; // divide
                }
            }

            // (AddOp +)  and  (SubOp -)
            while ((index = IndexOfEitherOr(operators, TokenType.AddOp, TokenType.SubOp)) >= 0) {
                TokenType op = operators[index]; operators.RemoveAt(index); // remove the operator
                double rhs = numbers[index + 1]; numbers.RemoveAt(index + 1); // remove the rhs number
                if (op == TokenType.AddOp) {
                    numbers[index] = numbers[index] + rhs; // multiply, and put into lhs number
                } else if (op == TokenType.SubOp) {
                    numbers[index] = numbers[index] - rhs; // divide
                }
            }

            // there should be no operators, and one number remaining
            if (operators.Count != 0 || numbers.Count != 1) {
                throw new NotImplementedException("Operator/number mismatch (end stage).");
            } else {
                return numbers[0];
            }
        }

        static int IndexOfEitherOr<T>(List<T> list, T first, T second) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].Equals(first) || list[i].Equals(second)) return i;
            }
            return -1;
        }
    }
}
