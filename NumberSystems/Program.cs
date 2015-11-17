using System;
using Numerics;
using Q = Numerics.BigRational;
using Z = System.Numerics.BigInteger;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace NumberSystems
{
    class Program
    {
        private static Dictionary<string, Q> constants;

        static void Main(string[] args)
        {
            constants = new Dictionary<string, Q>();
            constants.Add("e", new Q((decimal)Math.E));
            constants.Add("pi", new Q((decimal)Math.PI));

            constants.Add("$frac", 0);
            constants.Add("$precision", 30);
            constants.Add("$input_base", 10);
            constants.Add("$output_base", 10);

            Console.WriteLine("Type #usage for instructions");

            while (true)
            {
                string input = Console.ReadLine().ToLower();
                if (input.StartsWith("#"))
                {
                    switch (input)
                    {
                        case "#clear":
                            Console.Clear();
                            break;
                        case "#list":
                            foreach (KeyValuePair<string, Q> c in constants)
                                Console.WriteLine("{0}: {1}", c.Key, NumToString(c.Value, constants["$output_base"]));
                            break;
                        case "#reset":
                            constants.Clear();
                            constants.Add("e", new Q((decimal)Math.E));
                            constants.Add("pi", new Q((decimal)Math.PI));
                            constants.Add("$frac", 0);
                            constants.Add("$precision", 30);
                            constants.Add("$input_base", 10);
                            constants.Add("$output_base", 10);

                            Console.Clear();
                            foreach (KeyValuePair<string, Q> c in constants)
                                Console.WriteLine("{0}: {1}", c.Key, NumToString(c.Value, constants["$output_base"]));
                            break;
                        case "#usage":
                            Console.WriteLine("Use #clear to clear the terminal");
                            Console.WriteLine("Use #list  to list constants in use");
                            Console.WriteLine("Use #reset to initialize the working environment to default");
                            Console.WriteLine("Use the constants $precision, $input_base and $output_base to modify behaviour during calculations and parsing");
                            Console.WriteLine("To output numbers in fractional form, set $frac to nonzero");
                            Console.WriteLine("Always use parantheses around negative values: 5^(-2)");
                            Console.WriteLine();
                            break;

                    }
                }
                else
                {
                    Q num = ParseExpression(input, constants["$input_base"]);
                    Console.WriteLine(NumToString(num, constants["$output_base"]));
                }
            }
        }

        private static string NumToString(Q input, Q toBase)
        {
            if (constants["$frac"] == Q.Zero || input.Denominator == 1)     // Either is frac false or denominator > 1
            {
                StringBuilder sb = new StringBuilder();
                char[] digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZÆØÅ".ToCharArray();

                int sign = input.Sign;
                if (input < 0)
                    input = -input;

                int pow = 0; // Number of digits to the left of decimal separator

                while (input / Q.Pow(toBase, pow + 1) >= Q.One)
                    pow++;

                Z precision = -constants["$precision"].GetWholePart();
                Q divisor = Q.Pow(toBase, pow);

                for (int p = pow; p > precision; p--)
                {
                    Z coefficient = (input / divisor).GetWholePart();   // Take the quotient 
                    input -= coefficient * divisor;                     // Leaves only remainder

                    sb.Append(digits[(int)coefficient]);

                    divisor /= toBase;
                }

                sb.Insert(pow + 1, '.');
                if (sign < 0)
                    sb.Insert(0, '-');

                string res = sb.ToString().TrimEnd('0').TrimEnd('.');
                return res;
            }
            else
            {
                return NumToString(input.Numerator, toBase) + "/" + NumToString(input.Denominator, toBase);
            }
        }

        private static Q ParseExpression(string expr, Q _base, Dictionary<string, Q> vars = null)
        {
            if (vars == null)
                vars = new Dictionary<string, BigRational>();

            int i;

            i = expr.IndexOf(":=");
            if (i != -1)
            {
                string vn = expr.Remove(i).Trim();
                Q value = ParseExpression(expr.Substring(i + 2), 10);

                if (constants.ContainsKey(vn))
                    constants[vn] = value;
                else
                    constants.Add(vn, value);

                return value;
            }

            i = expr.LastIndexOf("(");
            if (i != -1)
            {
                int l = expr.Substring(i).IndexOf(")");
                if (l == -1)
                {
                    Console.WriteLine("Missing end paranthesis");
                    l = expr.Length - i;
                    expr = expr + ")";
                }

                Q prnt = ParseExpression(expr.Substring(i + 1, l - 1), _base, vars);

                string vname = "$v" + vars.Count; // Name of variable
                vars.Add(vname, prnt);

                expr = expr.Remove(i, l + 1);
                expr = expr.Insert(i, vname);

                return ParseExpression(expr, _base, vars);
            }

            i = expr.IndexOf("+");
            if (i != -1)
            {
                Q left = ParseExpression(expr.Remove(i), _base, vars);
                Q rght = ParseExpression(expr.Substring(i + 1), _base, vars);
                return left + rght;
            }

            i = expr.IndexOf("-");
            if (i != -1)
            {
                Q left = ParseExpression(expr.Remove(i), _base, vars);
                Q rght = ParseExpression(expr.Substring(i + 1), _base, vars);
                return left - rght;
            }

            i = expr.IndexOf("*");
            if (i != -1)
            {
                Q left = ParseExpression(expr.Remove(i), _base, vars);
                Q rght = ParseExpression(expr.Substring(i + 1), _base, vars);
                return left * rght;
            }

            i = expr.IndexOf("/");
            if (i != -1)
            {
                Q left = ParseExpression(expr.Remove(i), _base, vars);
                Q rght = ParseExpression(expr.Substring(i + 1), _base, vars);

                if (rght == Q.Zero)
                {
                    Console.WriteLine("ERROR - DIVIDE BY ZERO NOT POSSIBLE WITHOUT AN ACTIVATED DIVISION SIGIL");
                    return Q.Zero;
                }

                return left / rght;
            }

            i = expr.IndexOf("^");
            if (i != -1)
            {
                Q left = ParseExpression(expr.Remove(i), _base, vars);
                Q rght = ParseExpression(expr.Substring(i + 1), _base, vars);

                if (left == Q.Zero && rght.Sign < 0)
                    return ParseExpression("0/0", _base, vars);

                left = Q.Pow(left, rght.Numerator);
                return NRoot(left, rght.Denominator); // left^(x/y) = root(left^x, y)

                /*
                    left = Q.Pow(left, rght.Numerator);
                    return NRoot(left, rght.Denominator);
                */
            }

            // No operators - just parse number

            return ParseNum(expr, _base, vars);
        }

        private static Z Ceiling(Q fraction)
        {
            return (fraction.Numerator + fraction.Denominator - 1) / fraction.Denominator;
        }

        private static Z RoundWhole(Q fraction)
        {
            return (fraction.Numerator + fraction.Denominator / 2) / fraction.Denominator;
        }

        private static Q ParseNum(string num, Q _base, Dictionary<string, Q> vars)
        {
            if (num.Contains("jostein"))
            {
                Console.WriteLine("U FRIKIN FAGITT U FUKING THINK U KNO MATHS???");
                RektForm f = new RektForm();
                f.ShowDialog();

                return Z.Parse("11111111111111111111111");
            }

            if (num.StartsWith("root"))
            {
                string param1 = num.Substring(4).Split(',')[0];
                string param2 = num.Substring(4).Split(',')[1];

                Q p1 = ParseExpression(param1, _base, vars);
                Q p2 = ParseExpression(param2, _base, vars);

                return NRoot(p1, p2.GetWholePart());
            }

            if (string.IsNullOrWhiteSpace(num))
                return Q.Zero;

            if (vars.ContainsKey(num))
                return vars[num];

            if (constants.ContainsKey(num))
                return constants[num];

            string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".Remove((int)Ceiling(_base));

            int sign = 1;
            if (num.StartsWith("-"))
            {
                sign = -1;
                num = num.Remove(0, 1);
            }

            int i;
            i = num.IndexOfAny(new char[] { ',', '.' });
            if (i == -1)
                i = num.Length;
            else
                num = num.Remove(i, 1);

            int pow = i - 1;
            Q magnitude = Q.Pow(_base, pow);
            Q number = Q.Zero;

            foreach (char c in num)
            {
                int val = digits.IndexOf(c);
                if (val == -1)
                {
                    Console.WriteLine("Unknown digit {0} in base {1}", c, NumToString(_base, 10));
                    return Q.Zero;
                }
                else
                {
                    number += val * magnitude;
                    magnitude /= _base;         // pow--
                }
            }

            return number * sign;
        }


        private static Q NRoot(Q number, Z root)
        {
            Q epsilon;
            if (constants.ContainsKey("$epsilon"))
                epsilon = constants["$epsilon"];
            else
                epsilon = Q.One / Q.Pow(constants["$output_base"], constants["$precision"].GetWholePart());

            // pick 2 as first approximation
            Q guess;
            Q nextGuess = 2;

            do
            {
                guess = nextGuess;

                Q part = Q.Pow(guess, root - 1);

                nextGuess = guess - (part * guess - number) / (root * part);

            } while (Q.Abs(nextGuess - guess) > epsilon);

            return nextGuess;
        }
    }
}
