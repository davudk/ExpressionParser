using System;
using System.Collections.Generic;

namespace ExParser
{
    public class Instance {
        public Dictionary<string, Func<double[], double>> Functions { get; private set; }
        public Dictionary<string, double> Constants { get; private set; }

        public Instance() {
            Functions = new Dictionary<string, Func<double[], double>>();
            Constants = new Dictionary<string, double>();
        }

        public double Evaluate(string s) {
            Tokenizer tknr = new Tokenizer(s);

            return 0; // parsing soon
        }
    }
}
