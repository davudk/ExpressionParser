# ExpressionParser
An expression parser for C# that supports variables and functions.

### Examples
##### (Ex 1) Simple expression evaluation
```C#
using ExParser;
using System;
...

Instance instance = new Instance();
double result = instance.Evaluate("2 + ( 3 * 4 )");
Console.WriteLine("The result is: " + result);
```
##### (Ex 2) Adding custom functions and constants
```C#
Instance instance = new Instance();

instance.Functions.Add("count", (d) => d.Length);

instance.Functions.Add("sin", (d) => {
	if (d.Length == 1) return Math.Sin(d[0]);
	else throw new ArgumentException("The sin function requires exactly one parameter.");
});

instance.Constants.Add("PI", Math.PI);

double result = instance.Evaluate("sin(PI / 4)"); // 0.707... = sqrt(2) / 2
Console.WriteLine("The result is: " + result);
```
##### (Ex 3) Using the built-in scientific instance
```C#
Instance instance = Instance.CreateScientificInstance();
double result = instance.Evaluate("atan2(sqrt(3), 1) / PI * 180"); // 60
Console.WriteLine("The result is: " + result);
```

##### (Ex 4) Using the built-in scientific instance
```C#
Instance instance = new Instance {
	EvalConst = name => {
		return name.ToCharArray().Sum(c => c);
	}//,
	//EvalFunc = (name, args) => {
	//    return 0; // return something of type: double?
	//}
};

Func<string, double> eval = instance.Evaluate;

// because the name ABC does not exist,
// the function EvalConst is called (defined above)
Console.WriteLine("Result: " + eval("ABC")); // ABC is 65+66+67=198
```
The scientific instance contains the standard math functions:
**abs, ceiling, floor, round, rem, trunc, sqrt, sin, cos, tan, asin, acos, atan, atan2**, and of course the constants **PI** and **E**.
