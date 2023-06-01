// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// 
using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;
using System.IO;

namespace TransformFx
{
    partial class Program
    {
        // Given a json record, apply a fx transform to flatten it. 
        public static void TransformRecord(string inputJsonFile, FxRules rules, TextWriter outputWriter)
        {
            RecalcEngine engine = new RecalcEngine();

            var input = (RecordValue)FormulaValueJSON.FromJson(File.ReadAllText(inputJsonFile));

            outputWriter.WriteLine("Result:");
            foreach (var kv in rules.Rules)
            {
                var newField = kv.Name;
                var value = engine.Eval(kv.Formula, input);

                outputWriter.WriteLine($"  {newField}: {value.ToObject()}");
            }
        }
    }
}