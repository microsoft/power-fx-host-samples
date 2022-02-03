// Microsoft Power Fx Console Formula REPL
// 
// Console based Read-Eval-Print-Loop that supports variables and formula recalc
//
// Licensed under the MIT license

using System;
using System.Text.RegularExpressions;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Core.Public.Values;
using Microsoft.PowerFx.Core.Public.Types;

namespace PowerFxHostSamples
{
    class ConsoleRepl
    {
        private static RecalcEngine engine;

        static void ResetEngine()
        {
            engine = new RecalcEngine();
            engine.AddFunction(new HelpFunction());
            engine.AddFunction(new ResetFunction());
            engine.AddFunction(new ExitFunction());
        }

        public static void Main()
        {
            ResetEngine();

            Console.WriteLine("Microsoft Power Fx Console Formula REPL, Version 0.2");
            Console.WriteLine("Enter Excel formulas.  Use \"Help()\" for details.");
            
            // loop
            while (true)
            {
                // read
                Console.Write("\n> ");
                var expr = Console.ReadLine();

                try
                {
                    Match match;

                    // variable assignment: Set( <ident>, <expr> )
                    if ((match = Regex.Match(expr, @"^\s*Set\(\s*(?<ident>\w+)\s*,\s*(?<expr>.*)\)\s*$")).Success)
                    {
                        var r = engine.Eval(match.Groups["expr"].Value);
                        Console.WriteLine(match.Groups["ident"].Value + ": " + PrintResult(r));
                        engine.UpdateVariable(match.Groups["ident"].Value, r);
                    }

                    // formula definition: <ident> = <formula>
                    else if ((match = Regex.Match(expr, @"^\s*(?<ident>\w+)\s*=(?<formula>.*)$")).Success)
                        engine.SetFormula(match.Groups["ident"].Value, match.Groups["formula"].Value, OnUpdate);

                    // eval and print everything else, unless empty lines and single line comment (which do nothing)
                    else if (!Regex.IsMatch(expr, @"^\s*//") && Regex.IsMatch(expr, @"[^\s]"))
                    {
                        var result = engine.Eval(expr);

                        if (result is Microsoft.PowerFx.Core.Public.Values.ErrorValue errorValue)
                            throw new Exception("Error: " + errorValue.Errors[0].Message);
                        else
                            Console.WriteLine(PrintResult(result));
                    }
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();
                }
            }
        }

        static void OnUpdate(string name, FormulaValue newValue)
        {
            Console.Write($"{name}: ");
            if( newValue is ErrorValue errorValue)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + errorValue.Errors[0].Message);
                Console.ResetColor();
            }
            else
                Console.WriteLine( PrintResult(newValue) );
        }

        static string PrintResult(object value)
        {
            string resultString = "";

            if (value is RecordValue record)
            {
                var separator = "";
                resultString = "{";
                foreach (var field in record.Fields)
                {
                    resultString += separator + $"{field.Name}:";
                    resultString += PrintResult(field.Value);
                    separator = ", ";
                }
                resultString += "}";
            }
            else if (value is TableValue table)
            {
                int valueSeen = 0, recordsSeen = 0;
                string separator = "";

                // check if the table can be represented in simpler [ ] notation,
                //   where each element is a record with a field named Value.
                foreach (var row in table.Rows)
                {
                    recordsSeen++;
                    if (row.Value is RecordValue scanRecord)
                    {
                        foreach (var field in scanRecord.Fields)
                            if (field.Name == "Value")
                            {
                                valueSeen++;
                                resultString += separator + PrintResult(field.Value);
                                separator = ", ";
                            }
                            else
                                valueSeen = 0;
                    }
                    else
                        valueSeen = 0;
                }

                if (valueSeen == recordsSeen)
                    return ("[" + resultString + "]");
                else
                {
                    // no, table is more complex that a single column of Value fields,
                    //   requires full treatment
                    resultString = "Table(";
                    separator = "";
                    foreach (var row in table.Rows)
                    {
                        resultString += separator + PrintResult(row.Value);
                        separator = ", ";
                    }
                    resultString += ")";
                }
            }
            else if (value is ErrorValue errorValue)
                resultString = "<Error: " + errorValue.Errors[0].Message + ">";
            else if (value is StringValue str)
                resultString = "\"" + str.ToObject().ToString().Replace("\"","\"\"") + "\"";
            else if (value is FormulaValue fv)
                resultString = fv.ToObject().ToString();
            else
                throw new Exception("unexpected type in PrintResult");

            return(resultString);
        }

        private class ResetFunction : ReflectionFunction
        {
            public ResetFunction() : base("Reset", FormulaType.Boolean) { }

            public BooleanValue Execute()
            {
                ResetEngine();
                return FormulaValue.New(true);
            }
        }

        private class ExitFunction : ReflectionFunction
        {
            public ExitFunction() : base("Exit", FormulaType.Boolean) { }

            public BooleanValue Execute()
            {
                System.Environment.Exit(0);
                return FormulaValue.New(true);
            }
        }

        private class HelpFunction : ReflectionFunction
        {
            public HelpFunction() : base("Help", FormulaType.String) { }

            public StringValue Execute()
            {
                int column = 0;
                string funcList = "";
                foreach (string func in engine.GetAllFunctionNames())
                {
                    funcList += $"  {func,-14}";
                    if (++column % 5 == 0)
                        funcList += "\n";
                }
                funcList += "  Set";

                return FormulaValue.New( 
@"
Set( <identifier>, <expression> ) creates or changes a variable's value.
<identifier> = <expression> defines a formula with automatic recalc.
<expression> alone is evaluated and the result displayed.

Available functions (all are case sensitive):
" + funcList + @"

Available operators: = <> <= >= + - * / % && And || Or ! Not in exactin 

Record syntax is { < field >: < value >, ... } without quoted field names.
    Example: { Name: ""Joe"", Age: 29 }
Use the Table function for a list of records.  
    Example: Table( { Name: ""Joe"" }, { Name: ""Sally"" } )
Use [ <value>, ... ] for a single column table, field name is ""Value"".
    Examples: [ 1, 2, 3 ] 
Records and Tables can be arbitrarily nested.

Once a formula is defined or a variable's type is defined, it cannot be changed.
    Use the Reset() function to clear all formulas and variables.
"
                );
            }
        }
    }
}
