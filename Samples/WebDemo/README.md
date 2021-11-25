# Introduction 
Demonstrates a web page hosting the Power Fx Formula Bar and doing evaluation. 

![image](https://user-images.githubusercontent.com/1538900/143385087-c086a26c-9f0d-4989-b5b5-0fe489ebc314.png)


# Architecture
The formula bar is a client-side react control that wraps the[ Monaco editor](https://microsoft.github.io/monaco-editor/). 

It makes network calls to the server-side component which implements the [Language Service Protocol](https://docs.microsoft.com/en-us/visualstudio/extensibility/language-server-protocol) on top of Power Fx to provide IDE features like completion suggestions, error squiggles, and inline help. The LSP work is done in the `Microsoft.PowerFx.LanguageServerProtocol` nuget package.

The test site also makes calls to the server to evaluate the power fx expressions using the Power Fx interpreter in `Microsoft.PowerFx.Interpreter`.

# Getting Started
Dependencies: 

| What | Source | NuGet |
| --- | --- | --- |
| (Service) PowerFx.Core | https://github.com/microsoft/power-fx | https://www.nuget.org/packages/Microsoft.PowerFx.Core |
| (Client App) Formula Bar control | tbd | https://www.npmjs.com/package/@microsoft/power-fx-formulabar |

# Build locally 

1. Open and run `.\Service\PowerFxService.sln` in VS2019. 
Be sure you're running in VS2019 (not an earlier version), and that you run via "PowerFxService" and not via IISExpress. 
This will build and run the LSP and Eval service at https://localhost:5001
These are just backend APIs and don't produce any UI. 

2. Build the client-side formula bar in the `.\app` folder. 
- `npm install`
- `npm start`

This will launch a webpage at  http://localhost:3000 hosting the formula bar control. This page will call the service from step 1. 

# Deploy
1. in `.\app`, run `npm build`
This will produce static build files in the `.\app\build` directory. 
Copy these to the wwwroot. 

2. Deploy the service to the same site. 
