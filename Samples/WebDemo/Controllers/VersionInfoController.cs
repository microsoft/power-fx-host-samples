using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Project1.Controllers
{
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
                new VersionInfo { Key = "PowerFx", Value = fxVersion },
                new VersionInfo { Key = "Buildtime", Value = buildTime }
            };
        }
    }

    public class VersionInfo
    {
        public string Key { get; set; }
        public string Value { get; set;  }
    }
}
