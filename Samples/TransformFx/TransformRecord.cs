// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// 
using Microsoft.PowerFx;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.PowerFx.Types;

namespace TransformFx
{
    partial class Program
    {
        // Given a json record, apply a fx transform to flatten it. 
        public static void TransformRecord(
            string inputJsonFile,
            FxRules rules,
            TextWriter outputWriter)
        {
            RecalcEngine engine = new RecalcEngine();

            var input = (RecordValue)FormulaValue.FromJson(File.ReadAllText(inputJsonFile));

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