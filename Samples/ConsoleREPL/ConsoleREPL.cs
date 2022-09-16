// Microsoft Power Fx Console Formula REPL
// 
// Console based Read-Eval-Print-Loop that supports variables and formula recalc
//
// Licensed under the MIT license

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;
using Microsoft.PowerFx.Core;

namespace PowerFxHostSamples
{
    class ConsoleRepl
    {
        private static RecalcEngine engine;
        private static bool FormatTable = true;
        private const string OptionFormatTable = "FormatTable";

        static void ResetEngine()
        {
            Features toenable = 0;
            foreach (Features feature in (Features[])Enum.GetValues(typeof(Features)))
                toenable |= feature;

            var config = new PowerFxConfig( toenable );

            config.AddFunction(new HelpFunction());
            config.AddFunction(new ResetFunction());
            config.AddFunction(new ExitFunction());
            config.AddFunction(new OptionFunction());

            var OptionsSet = new OptionSet("Options", DisplayNameUtility.MakeUnique(new Dictionary<string, string>()
            {
                    { OptionFormatTable, OptionFormatTable },
            }));

            config.AddOptionSet(OptionsSet);

            engine = new RecalcEngine(config);   
        }

        public static void Main()
        {
            var expr = "";
            var exprPartial = "";
            var enabled = "";

            Microsoft.PowerFx.Preview.FeatureFlags.StringInterpolation = true;

            ResetEngine();

            var version = typeof(RecalcEngine).Assembly.GetName().Version.ToString();
            Console.WriteLine($"Microsoft Power Fx Console Formula REPL, Version {version}");

            foreach (Features feature in (Features[])Enum.GetValues(typeof(Features)))
                if ((engine.Config.Features & feature) == feature && feature != Features.None)
                    enabled += " " + feature.ToString();
            Console.WriteLine($"Experimental features enabled:{(enabled == "" ? " <none>" : enabled)}");

            Console.WriteLine($"Enter Excel formulas.  Use \"Help()\" for details.");
            
            // loop
            while (true)
            {
                Match match;

                // read
                if( exprPartial == "" )
                    Console.Write("\n> ");

                var exprOne = Console.ReadLine();
                if ((match = Regex.Match(expr, @"^(?<code>.*)//.*$")).Success )
                    exprOne = match.Groups["code"].Value;

                if (!Regex.IsMatch(exprOne, "^\\s*$"))
                {
                    exprPartial += " " + exprOne;
                    if( Regex.Matches(exprPartial, "[\\{\\(\\[]").Count != Regex.Matches(exprPartial, "[\\}\\)\\]]").Count || Regex.IsMatch(exprOne, "(=|=\\>)\\s*$") )
                        continue;
                }
                expr = exprPartial;
                exprPartial = "";

                try
                {
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

                    // function definition: <ident>( <ident> : <type>, ... ) : <type> = <formula>
                    //                      <ident>( <ident> : <type>, ... ) : <type> { <formula>; <formula>; ... }
                    else if (Regex.IsMatch(expr, @"^\s*\w+\((\s*\w+\s*\:\s*\w+\s*,?)*\)\s*\:\s*\w+\s*(\=|\{).*$"))
                    {
                        var res = engine.DefineFunctions(expr);
                        if( res.Errors.Count() > 0 )
                            throw new Exception("Error: " + res.Errors.First() );
                    }

                    // eval and print everything else, unless empty lines and single line comment (which do nothing)
                    else if (!Regex.IsMatch(expr, @"^\s*//") && Regex.IsMatch(expr, @"[^\s]"))
                    {
                        var result = engine.Eval(expr);

                        if (result is ErrorValue errorValue)
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
            if (newValue is ErrorValue errorValue)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + errorValue.Errors[0].Message);
                Console.ResetColor();
            }
            else
            {
                if (newValue is TableValue)
                    Console.WriteLine("");
                Console.WriteLine(PrintResult(newValue));
            }
        }

        static string PrintResult(object value, Boolean minimal = false)
        {
            string resultString = "";

            if(value is BlankValue)
                resultString = (minimal ? "" : "Blank()");
            else if (value is ErrorValue errorValue)
                resultString = (minimal ? "<error>" : "<Error: " + errorValue.Errors[0].Message + ">");
            else if (value is UntypedObjectValue)
                resultString = (minimal ? "<untyped>" : "<Untyped: Use Value, Text, Boolean, or other functions to establish the type>");
            else if (value is StringValue str)
                resultString = (minimal ? str.ToObject().ToString() : "\"" + str.ToObject().ToString().Replace("\"", "\"\"") + "\"");
            else if (value is RecordValue record)
            {
                if (minimal)
                    resultString = "<record>";
                else
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
            }
            else if (value is TableValue table)
            {
                if (minimal)
                    resultString = "<table>";
                else
                {
                    int[] columnWidth = new int[table.Rows.First().Value.Fields.Count()];

                    foreach (var row in table.Rows)
                    {
                        var column = 0;
                        foreach (var field in row.Value.Fields)
                        { 
                            columnWidth[column] = Math.Max(columnWidth[column], PrintResult(field.Value, true).Length);
                            column++;
                        }
                    }

                    // special treatment for single column table named Value
                    if (columnWidth.Length == 1 && table.Rows.First().Value.Fields.First().Name == "Value" ) 
                    {
                        string separator = "";
                        resultString = "[";
                        foreach (var row in table.Rows)
                        { 
                            resultString += separator + PrintResult(row.Value.Fields.First().Value);
                            separator = ", ";
                        }
                        resultString += "]";
                    }
                    // otherwise a full table treatment is needed
                    else if( FormatTable )
                    {
                        resultString = "\n ";
                        var column = 0;
                        foreach (var field in table.Rows.First().Value.Fields)
                        {
                            columnWidth[column] = Math.Max(columnWidth[column], field.Name.Length);
                            resultString += " " + field.Name.PadLeft(columnWidth[column]) + "  ";
                            column++;
                        }
                        resultString += "\n ";
                        foreach (var width in columnWidth)
                            resultString += new string('=', width+2) + " ";

                        foreach (var row in table.Rows)
                        {
                            column = 0;
                            resultString += "\n ";
                            foreach (var field in row.Value.Fields)
                            {
                                resultString += " " + PrintResult(field.Value, true).PadLeft(columnWidth[column]) + "  ";
                                column++;
                            }
                        }
                    }
                    // table without formatting 
                    else
                    {
                        resultString = "[";
                        string separator = "";
                        foreach (var row in table.Rows)
                        {
                            resultString += separator + PrintResult(row.Value);
                            separator = ", ";
                        }
                        resultString += "]";
                    }
                }
            }
            // must come last, as everything is a formula value
            else if (value is FormulaValue fv)
                resultString = fv.ToObject().ToString();
            else
                throw new Exception("unexpected type in PrintResult");

            return(resultString);
        }

        private class ResetFunction : ReflectionFunction
        {
            public BooleanValue Execute()
            {
                ResetEngine();
                return FormulaValue.New(true);
            }
        }

        private class ExitFunction : ReflectionFunction
        {
            public BooleanValue Execute()
            {
                System.Environment.Exit(0);
                return FormulaValue.New(true);
            }
        }
        
        private class OptionFunction : ReflectionFunction
        {
            public BooleanValue Execute(StringValue option, BooleanValue value)
            {
                if (option.Value.ToString().ToLower() == OptionFormatTable.ToLower() )
                {
                    FormatTable = value.Value;
                    return value;
                }
                return FormulaValue.New(false);
            }
        }
        
        private class HelpFunction : ReflectionFunction
        {
            public BooleanValue Execute()
            {
                int column = 0;
                string funcList = "";
                var funcNames = engine.Config.FunctionInfos.Select(x => x.Name).Distinct();
                foreach (string func in funcNames)
                {
                    funcList += $"  {func,-14}";
                    if (++column % 5 == 0)
                        funcList += "\n";
                }
                funcList += "  Set";
                
                // If we return a string, it gets escaped. 
                // Just write to console 
                Console.WriteLine(
                @"
<formula> alone is evaluated and the result displayed.
    Example: 1+1 or ""Hello, World""
Set( <identifier>, <formula> ) creates or changes a variable's value.
    Example: Set( x, x+1 )

<identifier> = <formula> defines a named formula with automatic recalc.
    Example: F = m * a
<identifier>( <param> : <type>, ... ) : <type> = <formula> 
        extends a named formula with parameters, creating a function.
    Example: F( m: Number, a: Number ): Number = m * a
<identifier>( <param> : <type>, ... ) : <type> { 
       <expression>; <expression>; ...
       }  defines a block function with chained formulas.
    Example: Log( message: String ): None { 
                    Collect( LogTable, message );
                    Notify( message );
             }
Supported types: Number, String, Boolean, DateTime, Date, Time

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
                return FormulaValue.New(true);
            }
        }
    }
}
