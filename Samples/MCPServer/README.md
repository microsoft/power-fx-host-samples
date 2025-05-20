# Simple Power Fx MCP Server

For VS Code, place this in your settings.json under "mcp" > "servers"

```
"PowerFx": {
    "type": "stdio",
    "command": "dotnet",
    "args": [
        "run",
        "--project",
        "c:\\path\\to\\repo\\Samples\\MCPServer\\MCPServer.csproj"
    ]
}
```

Test with 
```
Write and test a Power Fx formula to concatenate the printable ASCII characters in order.
```

For more information on C# MCP support, see:
- https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/#comments
- https://www.youtube.com/watch?v=iS25RFups4A
