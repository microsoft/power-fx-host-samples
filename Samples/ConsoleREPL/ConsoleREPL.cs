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
// using System.Security.Cryptography.X509Certificates;
using System.IO;
// using Microsoft.VisualBasic.FileIO;

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
            config.AddFunction(new ResetImportFunction());
            config.AddFunction(new ImportFunction());

            var OptionsSet = new OptionSet("Options", DisplayNameUtility.MakeUnique(new Dictionary<string, string>()
                                                {
                                                        { OptionFormatTable, OptionFormatTable },
                                                }
                                           ));

            config.AddOptionSet(OptionsSet);

            engine = new RecalcEngine(config);   
        }

        public static void Main()
        {
            string enabled = "";

            Microsoft.PowerFx.Preview.FeatureFlags.StringInterpolation = true;

            ResetEngine();

            var version = typeof(RecalcEngine).Assembly.GetName().Version.ToString();
            Console.WriteLine($"Microsoft Power Fx Console Formula REPL, Version {version}");

            foreach (Features feature in (Features[])Enum.GetValues(typeof(Features)))
                if ((engine.Config.Features & feature) == feature && feature != Features.None)
                    enabled += " " + feature.ToString();
            Console.WriteLine($"Experimental features enabled:{(enabled == "" ? " <none>" : enabled)}");

            Console.WriteLine($"Enter Excel formulas.  Use \"Help()\" for details.");

            REPL(Console.In, false);
        }

        public static void REPL(TextReader input, Boolean echo)
        {
            string expr;

            // main loop
            while ((expr = ReadFormula(input, echo)) != null )
            {
                Match match;

                try
                {
                    // variable assignment: Set( <ident>, <expr> )
                    if ((match = Regex.Match(expr, @"^\s*Set\(\s*(?<ident>\w+)\s*,\s*(?<expr>.*)\)", RegexOptions.Singleline)).Success)
                    {
                        var r = engine.Eval(match.Groups["expr"].Value);
                        Console.WriteLine(match.Groups["ident"].Value + ": " + PrintResult(r));
                        engine.UpdateVariable(match.Groups["ident"].Value, r);
                    }

                    // formula definition: <ident> = <formula>
                    else if ((match = Regex.Match(expr, @"^\s*(?<ident>\w+)\s*=(?<formula>.*)$", RegexOptions.Singleline)).Success)
                        engine.SetFormula(match.Groups["ident"].Value, match.Groups["formula"].Value, OnUpdate);

                    // function definition: <ident>( <ident> : <type>, ... ) : <type> = <formula>
                    //                      <ident>( <ident> : <type>, ... ) : <type> { <formula>; <formula>; ... }
                    else if (Regex.IsMatch(expr, @"^\s*\w+\((\s*\w+\s*\:\s*\w+\s*,?)*\)\s*\:\s*\w+\s*(\=|\{).*$", RegexOptions.Singleline))
                    {
                        var res = engine.DefineFunctions(expr);
                        if( res.Errors.Count() > 0 )
                            throw new Exception("Error: " + res.Errors.First() );
                    }

                    // eval and print everything else
                    else
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

        public static string ReadFormula(TextReader input, Boolean echo)
        {
            string exprPartial;
            int usefulCount;

            // read
            do
            {
                string exprOne;
                int parenCount;

                exprPartial = null;

                do
                {
                    bool doubleQuote, singleQuote;
                    bool lineComment, blockComment;
                    char last;

                    if (exprPartial == null && !echo)
                        Console.Write("\n> ");

                    exprOne = input.ReadLine();

                    if (exprOne == null)
                    {
                        Console.Write("\n");
                        return exprPartial;
                    }

                    exprPartial += exprOne + "\n";

                    // determines if the parens, curly braces, and square brackets are closed
                    // taking into escaping, block, and line comments
                    // and continues reading lines if they are not, with a blank link terminating
                    parenCount = 0;
                    doubleQuote = singleQuote = lineComment = blockComment = false;
                    last = '\0';
                    usefulCount = 0;
                    foreach (char c in exprPartial)
                    {
                        if (c == '"' && !singleQuote)         // don't need to worry about escaping as it looks like two 
                            doubleQuote = !doubleQuote;       // strings that are back to back
                        if (c == '\'' && !doubleQuote)
                            singleQuote = !singleQuote;
                        if (c == '*' && last == '/' && !blockComment)
                        {
                            blockComment = true;
                            usefulCount--;                         // compensates for the last character already being added
                        }
                        if (c == '/' && last == '*' && blockComment)
                        {
                            blockComment = false;
                            usefulCount--;
                        }
                        if (!doubleQuote && !singleQuote && !blockComment && !lineComment && c == '/' && last == '/')
                        {
                            lineComment = true;
                            usefulCount--;
                        }
                        if (c == '\n') lineComment = false;
                        if (!lineComment && !blockComment && !doubleQuote && !singleQuote)
                        {
                            if (c == '(' || c == '{' || c == '[') parenCount++;
                            if (c == ')' || c == '}' || c == ']') parenCount--;
                        }
                        if (!char.IsWhiteSpace(c) && !lineComment && !blockComment)
                            usefulCount++;
                        last = c;
                    }
                }
                while (!Regex.IsMatch(exprOne, "^\\s*$") && (parenCount != 0 || Regex.IsMatch(exprOne, "(=|=\\>)\\s*$")));

                if (echo && !Regex.IsMatch(exprPartial, "^\\s*$"))
                    Console.Write("\n>> " + exprPartial);
            }
            while (usefulCount == 0);

            return exprPartial;
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
            // explicit constructor needed so that the return type from Execute can be FormulaValue and acoomodate both booleans and errors
            public OptionFunction() : base("Option", FormulaType.Boolean, new [] { FormulaType.String, FormulaType.Boolean } ) { }

            public FormulaValue Execute(StringValue option, BooleanValue value)
            {
                if (option.Value.ToLower() == OptionFormatTable.ToLower())
                {
                    FormatTable = value.Value;
                    return value;
                }
                else
                {
                    return FormulaValue.NewError(new ExpressionError()
                        {
                            Kind = ErrorKind.InvalidArgument,
                            Severity = ErrorSeverity.Critical,
                            Message = $"Invalid option name: {option.Value}."
                        }
                    );
                }
            }
        }

        private class ImportFunction : ReflectionFunction
        {
            public ImportFunction() : base("Import", FormulaType.Boolean, new[] { FormulaType.String } ) { }

            public FormulaValue Execute(StringValue fileNameSV)
            {
                string fileName = fileNameSV.Value;
                if (File.Exists(fileName))
                {
                    TextReader fileReader = new StreamReader(fileName);
                    ConsoleRepl.REPL(fileReader, true);
                    fileReader.Close();
                }
                else
                {
                    return FormulaValue.NewError(new ExpressionError()
                    {
                        Kind = ErrorKind.InvalidArgument,
                        Severity = ErrorSeverity.Critical,
                        Message = $"File not found: {fileName}."
                    }
                    );
                }
                return FormulaValue.New(true);
            }
        }

        private class ResetImportFunction : ReflectionFunction
        {
            public ResetImportFunction() : base("ResetImport", FormulaType.Boolean, new[] { FormulaType.String }) { }

            public FormulaValue Execute(StringValue fileNameSV)
            {
                ImportFunction import = new ImportFunction();
                if (File.Exists(fileNameSV.Value))
                    ResetEngine();
                return import.Execute(fileNameSV);
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
    Example: Log( message: String ): None 
             { 
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
    Example: [ 1, 2, 3 ] 
Records and Tables can be arbitrarily nested.

Use Option( Options.FormatTable, false ) to disable table formatting.

Once a formula is defined or a variable's type is defined, it cannot be changed.
Use the Reset() function to clear all formulas and variables.
"
                );
                return FormulaValue.New(true);
            }
        }
    }
}
