// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.PowerFx.Types;
using System;
using System.Linq;
using System.Text;

namespace PowerFxService.Model
{
    // Helper to print results. 
    // This should get shared with Repl or from framework.
    public class PowerFxHelper
    {
        internal static string TestToString(FormulaValue result)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                TestToString(result, sb);
            }
            catch (Exception e)
            {
                // This will cause a diff and test failure below. 
                sb.Append($"<exception writing result: {e.Message}>");
            }

            string actualStr = sb.ToString();
            return actualStr;
        }

        // $$$ Move onto FormulaValue. 
        // Result here should be a string value that could be parsed. 
        // Normalize so we can use this in test cases. 
        internal static void TestToString(FormulaValue result, StringBuilder sb)
        {
            if (result is DecimalValue dec)
            {
                sb.Append(dec.Value);
            }
            else if (result is NumberValue n)
            {
                sb.Append(n.Value);
            }
            else if (result is BooleanValue b)
            {
                sb.Append(b.Value ? "true" : "false");
            }
            else if (result is StringValue s)
            {
                // $$$ proper escaping?
                sb.Append('"' + s.Value + '"');
            }
            else if (result is TableValue t)
            {
                var tableType = (TableType)t.Type;
                var canUseSquareBracketSyntax = t.IsColumn && t.Rows.All(r => r.IsValue) && tableType.FieldNames.First() == "Value";
                if (canUseSquareBracketSyntax)
                {
                    sb.Append('[');
                }
                else
                {
                    sb.Append("Table(");
                }

                var dil = string.Empty;
                foreach (var row in t.Rows)
                {
                    sb.Append(dil);
                    dil = ",";

                    if (canUseSquareBracketSyntax)
                    {
                        var val = row.Value.Fields.First().Value;
                        TestToString(val, sb);
                    }
                    else
                    {
                        if (row.IsValue)
                        {
                            TestToString(row.Value, sb);
                        }
                        else
                        {
                            TestToString(row.ToFormulaValue(), sb);
                        }
                    }

                    dil = ",";
                }

                if (canUseSquareBracketSyntax)
                {
                    sb.Append(']');
                }
                else
                {
                    sb.Append(')');
                }
            }
            else if (result is RecordValue r)
            {
                var fields = r.Fields.ToArray();
                Array.Sort(fields, (a, b) => string.CompareOrdinal(a.Name, b.Name));

                sb.Append('{');
                var dil = string.Empty;

                foreach (var field in fields)
                {
                    sb.Append(dil);
                    sb.Append(field.Name);
                    sb.Append(':');
                    TestToString(field.Value, sb);

                    dil = ",";
                }

                sb.Append('}');
            }
            else if (result is TimeValue tv)
            {
                sb.Append(tv.Value.ToString());
            }
            else if (result is BlankValue)
            {
                sb.Append("Blank()");
            }
            else if (result is DateValue d)
            {
                // Date(YYYY,MM,DD)
                var date = d.Value;
                sb.Append($"Date({date.Year},{date.Month},{date.Day})");
            }
            else if (result is DateTimeValue dt)
            {
                // DateTime(yyyy,MM,dd,HH,mm,ss,fff)
                var dateTime = dt.Value;
                sb.Append($"DateTime({dateTime.Year},{dateTime.Month},{dateTime.Day},{dateTime.Hour},{dateTime.Minute},{dateTime.Second},{dateTime.Millisecond})");
            }
            else if (result is ErrorValue)
            {
                sb.Append(result);
            }
            else
            {
                throw new InvalidOperationException($"unsupported value type: {result.GetType().Name}");
            }
        }
    }
}
