using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Project1.Controllers
{
    // Show versions of various components used for 
    [ApiController]
    [Route("[controller]")]
    public class VersionInfoController : ControllerBase
    {
        [HttpGet]
        public VersionInfo[] Get()
        {
            var fxVersion = typeof(Microsoft.PowerFx.RecalcEngine).Assembly.GetName().Version.ToString();

            Assembly asm = Assembly.GetExecutingAssembly();
            FileInfo fi = new FileInfo(asm.Location);
            var buildTime = fi.CreationTimeUtc.ToString() + " UTC";
                        

            return new VersionInfo[]
            {
                new VersionInfo { Key = "Buildtime", Value = buildTime, Url = "https://github.com/microsoft/power-fx-host-samples/tree/main/Samples/WebDemo" },
                new VersionInfo { Key = "Power Fx", Value = fxVersion, Url="https://github.com/microsoft/power-fx" },
                new VersionInfo { Key = "Formula Bar Control", Value=GetFormulaBarVersion(), Url = "https://www.npmjs.com/package/@microsoft/power-fx-formulabar" }
            };
        }

        // Formula Bar is "@microsoft/power-fx-formulabar" in Package.Json. 
        // Include package.json as an embedded resource so we can get access to it.
        private string GetFormulaBarVersion()
        {
            var name = "WebDemo.Resources.package.json";
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name))
            {
                var text = new StreamReader(stream).ReadToEnd();
                var lines = text.Split('\n');

                var line = lines.Where(x => x.Contains("power-fx-formulabar")).First();
                line = line.Replace("\"", "").Replace(",", "");                                

                return line;
            }                
        }
    }

    public class VersionInfo
    {
        public string Key { get; set; }
        public string Value { get; set;  }
        public string Url { get; set; }
    }
}
