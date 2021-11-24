// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.PowerFx.Core.Public.Values;
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
            if (result is NumberValue n)
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
                if (t.IsColumn)
                {
                    sb.Append('[');

                    string dil = "";
                    foreach (DValue<RecordValue> row in t.Rows)
                    {
                        sb.Append(dil);
                        var val = row.Value.Fields.First().Value;
                        TestToString(val, sb);
                        dil = ",";
                    }
                    sb.Append(']');
                }
                else
                {
                    sb.Append('[');

                    string dil = "";
                    foreach (DValue<RecordValue> row in t.Rows)
                    {
                        sb.Append(dil);
                        TestToString(row.Value, sb);
                        dil = ",";
                    }
                    sb.Append(']');
                }
            }
            else if (result is RecordValue r)
            {
                // If this is a single record with field Value, then show as 
                // [x] instead of  {value : x}

                var fields = r.Fields.ToArray();
                if (fields.Length == 1)
                {
                    if (fields[0].Name == "Value")
                    {
                        sb.Append('[');
                        TestToString(fields[0].Value, sb);
                        sb.Append(']');
                        return;
                    }
                }

                Array.Sort(fields, (a, b) => string.CompareOrdinal(a.Name, b.Name));

                sb.Append('{');
                string dil = "";

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
            else if (result is BlankValue)
            {
                sb.Append("Blank()");
            }
            else
            {
                throw new InvalidOperationException($"unsupported value type: {result.GetType().Name}");
            }
        }
    }
}
