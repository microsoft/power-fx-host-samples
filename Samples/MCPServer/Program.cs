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
    [McpServerTool, Description("Evaluates a Power Fx formula and returns the result.")]
    public static string Evaluate(string message)
    {
        var config = new PowerFxConfig();
        var engine = new RecalcEngine(config);
        var result = engine.Eval(message);
        return result.ToExpression();
    }
}
