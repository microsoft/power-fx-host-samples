// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// 
// Demonstration of using Power Fx for transforms. 
using Microsoft.PowerFx;
using Microsoft.PowerFx.Core.Public.Values;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace TransformFx
{
    partial class Program
    {
        // Create new columns in the csv defined by the Power Fx functions. 
        // Apply on each row. 
        public static void TransformCsv(
            string inputCsvFile,
             FxRules rules,
            TextWriter outputWriter)
        {
            var columns = rules.Rules;

            RecalcEngine engine = new RecalcEngine();


            var lines = File.ReadAllLines(inputCsvFile);

            var headers = ParseCsvLine(lines[0]);

            TextWriter tw = outputWriter;

            // Write headers
            {
                tw.Write(string.Join(",", headers));
                foreach (var kv in columns)
                {
                    tw.Write(",");
                    tw.Write(kv.Name);
                }
                tw.WriteLine();
            }

            RecordValue prevRecord;
            // First record is all zeros. 
            {
                List<NamedValue> fields = new List<NamedValue>();
                for (int i = 0; i < headers.Length; i++)
                {
                    var columnName = headers[i];
                    var value = FormulaValue.New(0); // default value 

                    fields.Add(new NamedValue(columnName, value));
                }
                foreach (var kv in columns)
                {
                    var columnName = kv.Name;
                    var value = FormulaValue.New(0); // default value 
                    fields.Add(new NamedValue(columnName, value));
                }
                prevRecord = RecordValue.RecordFromFields(fields);
            }

            // Process each line
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                List<NamedValue> fields = new List<NamedValue>();
                fields.Add(new NamedValue("PreviousRecord", prevRecord));

                var csvValues = ParseCsvLine(line);
                for (int i = 0; i < headers.Length; i++)
                {
                    var columnName = headers[i];
                    var value = ParseCsvValue(csvValues[i]);

                    fields.Add(new NamedValue(columnName, value));
                }

                foreach (var kv in columns)
                {
                    var fv = RecordValue.RecordFromFields(fields);

                    var result = engine.Eval(kv.Formula, fv);

                    fields.Add(new NamedValue(kv.Name, result));
                }

                // Write out existing values
                // skip past 1st to exclude PrevRecord
                var thisFields = fields.Skip(1);
                prevRecord = RecordValue.RecordFromFields(thisFields);

                // Write out full row.                 
                string dil = "";
                foreach (var item in thisFields)
                {
                    tw.Write(dil);
                    tw.Write(item.Value.ToObject());
                    dil = ",";
                }
                tw.WriteLine();
            }
        }

        // TODO - use a Real CSV parser, not just string.split.
        static string[] ParseCsvLine(string line)
        {
            return line.Split(',');
        }

        // Parse a value in the CSV 
        // Must use pattern matching if we don't have a true schema. 
        static FormulaValue ParseCsvValue(string str)
        {
            if (double.TryParse(str, out var d))
            {
                return FormulaValue.New(d);
            }
            return FormulaValue.New(str);
        }
    }
}