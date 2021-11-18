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
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // $$$ Hoist common Fx parser here. 
            // Switch based on input. 
            // $$$ Add a readme with samples. 
            // rename samples. 

            // foo.json transform.fx.yaml
            // foo.csv transform.fx.yaml

            TransformRecord(
                @"D:\dev\pa2\power-fx-host-samples\Samples\TransformFx\sampleData\data.json",
                @"D:\dev\pa2\power-fx-host-samples\Samples\TransformFx\sampleData\JsonTransform.fx.yaml",
                Console.Out
                );

            TransformCsv(
                @"D:\dev\pa2\power-fx-host-samples\Samples\TransformFx\sampleData\data.csv",
                @"D:\dev\pa2\power-fx-host-samples\Samples\TransformFx\sampleData\Test.fx.yaml",
                Console.Out);

        }

        // Given a json record, apply a fx transform to flatten it. 
        public static void TransformRecord(
            string inputJsonFile,
            string fxYamlFile,
            TextWriter outputWriter)
        {
            RecalcEngine engine = new RecalcEngine();

            var columns = new FxParser().ReadColumns(fxYamlFile);

            var input = (RecordValue)FormulaValue.FromJson(File.ReadAllText(inputJsonFile));

            outputWriter.WriteLine("Result:");
            foreach (var kv in columns)
            {
                var newField = kv.Name;
                var value = engine.Eval(kv.Formula, input);

                outputWriter.WriteLine($"  {newField}: {value.ToObject()}");
            }
        }

        // Create new columns in the csv defined by the Power Fx functions. 
        // Apply on each row. 
        public static void TransformCsv(
            string inputCsvFile,
            string fxYamlFile,
            TextWriter outputWriter)
        {
            var columns = new FxParser().ReadColumns(fxYamlFile);

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
