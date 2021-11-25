# Introduction 
Demonstrates Power Fx Formula Bar with intellisense. 

This is a reusable React Control that provides errors and intellisense around Power Fx.  
It can be used by Web Apps that embed power fx formulas. 

# Architecture
The formula bar is a client-side react control that wraps monaco. 

It makes network calls to the server-side component which implements the [Language Service Protocol](https://docs.microsoft.com/en-us/visualstudio/extensibility/language-server-protocol) on top of Power Fx. This is done in the `Microsoft.PowerFx.LanguageServerProtocol` nuget package. 

The test site also makes calls to the server to evaluate the power fx expressions using the Power Fx interpreter in `Microsoft.PowerFx.Interpreter`.


# Getting Started
Dependencies: 

| What | Source | NuGet |
| --- | --- | --- |
| (Service) PowerFx.Core | https://github.com/microsoft/power-fx | https://www.nuget.org/packages/Microsoft.PowerFx.Core |
| (Client App) Formula Bar control | tbd | https://www.npmjs.com/package/@microsoft/power-fx-formulabar |

# Build locally 

1. Run the server side part `.\Service\PowerFxService.sln`
This will build and run the LSP and Eval service at https://localhost:5001

2. Build the client-side formula bar in the `.\app` folder. 
- `npm install`
- `npm start`

This will launch a webpage at  http://localhost:3000 hosting the formula bar control.   This page will call the service from step 1. 

# Deploy
1. in `.\app`, run `npm build`
This will produce static build files in the `.\app\build` directory. 
Copy these to the wwwroot. 

2. Deploy the service to the same site. 
