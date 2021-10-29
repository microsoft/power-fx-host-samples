// Microsoft Power Fx Console Formula REPL
// 
// Console based Read-Eval-Print-Loop that supports variables and formula recalc
//
// Licensed under the MIT license

using System;
using System.Text.RegularExpressions;
using Microsoft.PowerFx;

namespace PowerFxHostSamples
{
    class ConsoleRepl
    {
        public static void Main()
        {
            var engine = new RecalcEngine();

            Console.Write("Microsoft Power Fx Console Formula REPL, Version 0.2\n");
            Console.Write("Commands: //help, //reset, and //exit\n");
            
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

                    // exit
                    else if (Regex.IsMatch(expr, @"\s*//\s*exit\s*$", RegexOptions.IgnoreCase))
                        return;

                    // reset
                    else if (Regex.IsMatch(expr, @"\s*//\s*reset\s*$", RegexOptions.IgnoreCase))
                    {
                        engine = new RecalcEngine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Formula engine has been reset.");
                        Console.ResetColor();
                    }

                    // help, includes list of all functions
                    else if (Regex.IsMatch(expr, @"^\s*//\s*help\s*$", RegexOptions.IgnoreCase))
                    {
                        int column = 0;
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("");
                        Console.WriteLine("Set( <identifier>, <expression> ) creates or changes a variable's value.\n");
                        Console.WriteLine("<identifier> = <expression> defines a formula.\n  If a dependency in the formula changes, the value of the formula is updated.\n  Formulas cannot be redefined.\n");
                        Console.WriteLine("Available functions (case sensitive):");
                        foreach (string func in engine.GetAllFunctionNames())
                        {
                            Console.Write($"  {func,-14}");
                            if (++column % 5 == 0)
                                Console.WriteLine();
                        }
                        Console.WriteLine("  Set\n");
                        Console.WriteLine("Available operators: = <> <= >= + - * / % in exactin && And || Or ! Not\n");
                        Console.WriteLine("Records are written { <field>: <value>, ... } without quotes around the field name.");
                        Console.WriteLine("Use the Table function for a list of records. Use [<value>,...] for a single column table.");
                        Console.WriteLine("  Examples: { Name: \"Joe\", Age: 29 }, [ 1, 2, 3 ], ");
                        Console.WriteLine("            Table( { Name: \"Joe\" }, { Name: \"Sally\" } )");
                        Console.ResetColor();
                    }

                    // eval and print everything else, unless empty lines and single line comment (which do nothing)
                    else if (!Regex.IsMatch(expr, @"^\s*//") && Regex.IsMatch(expr, @"\w"))
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

        static void OnUpdate(string name, Microsoft.PowerFx.Core.Public.Values.FormulaValue newValue)
        {
            Console.Write($"{name}: ");
            if( newValue is Microsoft.PowerFx.Core.Public.Values.ErrorValue errorValue)
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

            if (value is Microsoft.PowerFx.Core.Public.Values.RecordValue record)
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
            else if (value is Microsoft.PowerFx.Core.Public.Values.TableValue table)
            {
                int valueSeen = 0, recordsSeen = 0;
                string separator = "";

                // check if the table can be represented in simpler [ ] notation,
                //   where each element is a record with a field named Value.
                foreach (var row in table.Rows)
                {
                    recordsSeen++;
                    if (row.Value is Microsoft.PowerFx.Core.Public.Values.RecordValue scanRecord)
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
            else if (value is Microsoft.PowerFx.Core.Public.Values.ErrorValue errorValue)
                resultString = "<Error: " + errorValue.Errors[0].Message + ">";
            else if (value is Microsoft.PowerFx.Core.Public.Values.StringValue str)
                resultString = "\"" + str.ToObject().ToString().Replace("\"","\"\"") + "\"";
            else if (value is Microsoft.PowerFx.Core.Public.Values.FormulaValue fv)
                resultString = fv.ToObject().ToString();
            else
                throw new Exception("unexpected type in PrintResult");

            return(resultString);
        }
    }
}
