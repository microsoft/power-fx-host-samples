// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TransformFx
{
    // Read a .fx.yaml file and return a group of formulas. 
    // This can produce a flat (non-nested) record where each field is defined by a Fx formula.
    class FxRules
    {
        private Rule[] _formulas;

        public IEnumerable<Rule> Rules => _formulas;

        // Describe each rule in the file. 
        public class Rule
        {
            public string Name;
            public string Formula;
        }

        // TODO - share this with Yaml parser in https://github.com/microsoft/PowerApps-Language-Tooling 
        // File is "Name: =formula"
        // Should make this a .fx.yaml
        public static FxRules ParseYamlFile(string file)
        {
            var lines = File.ReadAllLines(file);

            List<Rule> columns = new List<Rule>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                if (line[0] == '#') { continue; }

                var split = ": =";
                var i = line.IndexOf(split);
                var name = line.Substring(0, i);
                var formula = line.Substring(i + split.Length);

                columns.Add(new Rule
                {
                    Name = name,
                    Formula = formula.Trim()
                });
            }
            return new FxRules
            {
                 _formulas = columns.ToArray()
            };
        }
    }
}
