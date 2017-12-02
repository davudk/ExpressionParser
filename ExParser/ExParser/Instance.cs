using System;
using System.Collections.Generic;

namespace ExParser {
    public delegate double ExFunc(params double[] args);
    public delegate double? EvalConst(string name);
    public delegate double? EvalFunc(string name, double[] args);
    public class Instance {
        public Dictionary<string, ExFunc> Functions { get; private set; }
        public Dictionary<string, double> Constants { get; private set; }
        public EvalConst EvalConst { get; set; }
        public EvalFunc EvalFunc { get; set; }

        public Instance() {
            Functions = new Dictionary<string, ExFunc>();
            Constants = new Dictionary<string, double>();
        }

        public double Evaluate(string s) {
            return Evaluate(new Tokenizer(s));
        }
        double Evaluate(ITokenReader tknr) {
            List<double> numbers = new List<double>();
            List<TokenType> operators = new List<TokenType>();

            bool makeNegative = false;
            Token tok;
            while ((tok = tknr.GetToken()) != null) {
                switch (tok.Type) {
                case TokenType.Number: {
                        if (double.TryParse(tok.Lexeme, out var value)) {
                            if (makeNegative) {
                                value *= -1;
                            }
                            numbers.Add(value);
                            makeNegative = false;
                        } else {
                            throw new NotImplementedException("Unable to parse double.");
                        }
                    }
                    break;
                case TokenType.LParen: {
                        List<Token> tokens = new List<Token>();
                        int level = 1;
                        while ((tok = tknr.GetToken()) != null) { // get the tokens within the parenthesis
                            if (tok.Type == TokenType.LParen) {
                                level += 1;
                            } else if (tok.Type == TokenType.RParen) {
                                level -= 1;
                                if (level == 0) break;
                            }
                            tokens.Add(tok); // add them to the token buffer
                        }

                        if (level != 0) {
                            throw new NotImplementedException("End of parenthesis was not met.");
                        }

                        // evaluate the token buffer recursively
                        double res = Evaluate(new TokenBuffer(tokens));
                        // add result to numbers list
                        numbers.Add(res);
                    }
                    break;
                case TokenType.SubOp:
                    if (operators.Count == numbers.Count) {
                        makeNegative = true;
                    } else {
                        operators.Add(tok.Type);
                    }
                    break;
                case TokenType.AddOp:
                case TokenType.MultOp:
                case TokenType.DivOp:
                case TokenType.PowOp:
                    operators.Add(tok.Type);
                    break;
                case TokenType.Label: { // either a constant or a function call
                        string name = tok.Lexeme;
                        double? final;
                        if ((tok = tknr.PeekToken()) != null && tok.Type == TokenType.LParen) { // function call
                            tknr.GetToken(); // get the left paren out of the way

                            List<double> paramNumbers = new List<double>();

                            // since this is a function call, all the arguments have to be evaluated, one by one
                            bool endArgs = false;
                            do {
                                List<Token> tokens = new List<Token>();

                                int level = 0;
                                bool commaMet = false;
                                while ((tok = tknr.GetToken()) != null) { // get the tokens in the current argument
                                    if (tok.Type == TokenType.LParen) {
                                        level += 1;
                                    } else if (tok.Type == TokenType.RParen) {
                                        if (level == 0) {
                                            endArgs = commaMet = true;
                                            break;
                                        }
                                        level -= 1;
                                    } else if (level == 0 && tok.Type == TokenType.Comma) {
                                        // end of the current argument, since the comma was hit
                                        commaMet = true;
                                        break;
                                    }
                                    tokens.Add(tok); // add them to the token buffer
                                }

                                if (level != 0 || commaMet == false) {
                                    throw new NotImplementedException("End of argument (comma) was not met.");
                                }

                                if (tokens.Count > 0) {
                                    // last read token was a comma
                                    // evaluate the current argument and put it into the parameters list
                                    // evaluate the token buffer
                                    double res = Evaluate(new TokenBuffer(tokens));
                                    // add result to numbers list
                                    paramNumbers.Add(res);
                                } else if (paramNumbers.Count > 0) {
                                    throw new NotImplementedException("Empty parameter is not allowed.");
                                }

                            } while (!endArgs);

                            if (Functions.TryGetValue(name, out var func)) { // retrieve
                                // evaluate the token buffer
                                double res = func(paramNumbers.ToArray());
                                // add result to numbers list
                                numbers.Add(res);
                            } else if ((final = EvalFunc?.Invoke(name, paramNumbers.ToArray())).HasValue) { // eval function
                                numbers.Add(final.Value);
                            } else {
                                throw new NotImplementedException("Function \"" + name + "\" does not exist.");
                            }
                        } else if (Constants.TryGetValue(name, out var value)) { // constant
                            numbers.Add(value);
                        } else if ((final = EvalConst?.Invoke(name)).HasValue) { // eval constant
                            numbers.Add(final.Value);
                        } else { // constant, but it doesn't exist in the constant map
                            throw new NotImplementedException("Constant \"" + name + "\" does not exist.");
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

        public static Instance CreateScientificInstance() {
            Instance ins = new Instance();

            // Simple functions
            ins.Functions.Add("abs", (args) => {
                if (args.Length == 1) return Math.Abs(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("ceiling", (args) => {
                if (args.Length == 1) return Math.Ceiling(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("floor", (args) => {
                if (args.Length == 1) return Math.Floor(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("round", (args) => {
                if (args.Length == 1) return Math.Round(args[0]);
                else if (args.Length == 2) return Math.Round(args[0], (int)args[1]);
                else throw new ArgumentOutOfRangeException("args", "This function requires one or two arguments.");
            });
            ins.Functions.Add("rem", (args) => {
                if (args.Length == 1) return args[0] % args[1];
                else throw new ArgumentOutOfRangeException("args", "This function requires two arguments.");
            });
            ins.Functions.Add("truncate", (args) => {
                if (args.Length == 1) return Math.Truncate(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("sqrt", (args) => {
                if (args.Length == 1) return Math.Sqrt(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });

            // Trig functions
            ins.Functions.Add("sin", (args) => {
                if (args.Length == 1) return Math.Sin(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("cos", (args) => {
                if (args.Length == 1) return Math.Cos(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("tan", (args) => {
                if (args.Length == 1) return Math.Tan(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });

            // Inverse trig functions
            ins.Functions.Add("asin", (args) => {
                if (args.Length == 1) return Math.Asin(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("acos", (args) => {
                if (args.Length == 1) return Math.Acos(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("atan", (args) => {
                if (args.Length == 1) return Math.Atan(args[0]);
                else throw new ArgumentOutOfRangeException("args", "This function requires exactly one argument.");
            });
            ins.Functions.Add("atan2", (args) => {
                if (args.Length == 2) return Math.Atan2(args[0], args[1]);
                else throw new ArgumentOutOfRangeException("args", "This function requires two arguments.");
            });

            // Constants
            ins.Constants.Add("E", Math.E);
            ins.Constants.Add("PI", Math.PI);

            return ins;
        }
    }
}
