// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TransformFx
{
    // Read a .fx.yaml file and return a group of formulas. 
    class FxParser
    {
        public class ColumnDescr
        {
            public string Name;
            public string Formula;
        }

        // TODO - share this with Yaml parser in https://github.com/microsoft/PowerApps-Language-Tooling 
        // File is "Name: =formula"
        // Should make this a .fx.yaml
        public ColumnDescr[] ReadColumns(string file)
        {
            var lines = File.ReadAllLines(file);

            List<ColumnDescr> columns = new List<ColumnDescr>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) { continue; }
                if (line[0] == '#') { continue; }

                var split = ": =";
                var i = line.IndexOf(split);
                var name = line.Substring(0, i);
                var formula = line.Substring(i + split.Length);

                columns.Add(new ColumnDescr
                {
                    Name = name,
                    Formula = formula.Trim()
                });
            }
            return columns.ToArray();
        }
    }
}
