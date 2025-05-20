using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Microsoft.PowerFx.Core;
using Microsoft.PowerFx.Repl;
using Microsoft.PowerFx.Repl.Functions;
using Microsoft.PowerFx.Repl.Services;
using Microsoft.PowerFx.Types;
using System.ComponentModel;
using Microsoft.PowerFx;
using System.Globalization;
using System.Diagnostics;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class PowerFxTool
{
    [McpServerTool, Description("Evaluate a Power Fx formula and return the result as a JSON string.")]
    public static string Evaluate(string message)
    {
        try
        {
            Console.Error.WriteLine($"Evaluate In: {message}");

            var config = new PowerFxConfig()
            {
                MaximumExpressionLength = 10000
            };
            config.EnableJsonFunctions();
            config.EnableRegExFunctions();
            var engine = new RecalcEngine(config);

            var resultVal = engine.Eval($"JSON({message})");
            var resultStr = ((StringValue)resultVal).Value;

            Console.Error.WriteLine($"Evaluate Out: {resultStr}");
            return resultStr;
        }
        catch (Exception ex)
        {
            var errorStr = $"ERROR: {ex.Message}";
            Console.Error.WriteLine(errorStr);
            return errorStr;
        }
    }
}
