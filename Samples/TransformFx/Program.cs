// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// 
// Demonstration of using Power Fx for transforms. 
using System;
using System.IO;

namespace TransformFx
{
    partial class Program
    {
        static void Main(string[] args)
        {            
            string argFile= args[0];
            string argFxYaml = args[1];

            var fxRules = FxRules.ParseYamlFile(argFxYaml);
            switch(Path.GetExtension(argFile).ToLower())
            {
                case ".json":
                    Console.WriteLine($"Transform Record:");
                    TransformRecord(argFile, fxRules, Console.Out);
                    break;

                case ".csv":
                    Console.WriteLine($"Transform Table:");
                    TransformCsv(argFile, fxRules, Console.Out);
                    break;

                default:
                    throw new Exception($"Unsupported file type: " + argFile);
            }
        }        
    }
}
