# Introduction 
Demonstrates a web page hosting the Power Fx Formula Bar and doing evaluation. 
The formula bar is a client-side react control that wraps the[ Monaco editor](https://microsoft.github.io/monaco-editor/) to provide features like error squiggles, completion suggestions, and function tool tips. 

![image](https://user-images.githubusercontent.com/1538900/143385087-c086a26c-9f0d-4989-b5b5-0fe489ebc314.png)

# Consuming the formula bar

The Power Fx formula bar is provided via a react control in the [@microsoft/power-fx-formulabar](https://www.npmjs.com/package/@microsoft/power-fx-formulabar) npm package.

```
        <PowerFxFormulaEditor
          getDocumentUriAsync={this._getDocumentUriAsync}
          defaultValue={''}
          messageProcessor={this._messageProcessor}
          maxLineCount={4}
          minLineCount={4}
          onChange={(newValue: string): void => {
            this.setState({ expression: newValue, hasErrors: false });
            this._evalAsync(context, newValue);
          }}
          onEditorDidMount={(editor, _): void => { this._editor = editor }}
          lspConfig={{
            enableSignatureHelpRequest: true
          }}
        />
```        

`getDocumentUriAsync` is used to provide a string with context for this formula bar which will get passed to the language server and used to recreate a `RecalcEngine` with appropriate symbols and context. This context can be any arbitrary uri string (including query parameters) and so it enables  multiple formula bars as well as passing in client-side state. 

`messageProcessor` is used to communicate to the LSP. The Power Fx compiler is implemented in C# and so the client-side formula bar must make network requests to consume the server-side LSP. That could be any network channel such as web sockets, signal R, or  rest. 

## Flow for intellisense:

1. The client page instantiates a <PowerFxFormulaEditor> control and provides context via getDocumentUriAsync. 
1. User edits text in the <PowerFxFormulaEditor> control. 
1. The control sends a LSP message via `messageProcessor`
1. That message is received server side by the ASP controller [LanguageServerController.HandleRequest](https://github.com/microsoft/power-fx-host-samples/blob/main/Samples/WebDemo/service/Controllers/LanguageServerController.cs). 
1. The controller is just a lightweight controller that passes the message through to the [LanguageServer](https://github.com/microsoft/Power-Fx/blob/main/src/libraries/Microsoft.PowerFx.LanguageServerProtocol/LanguageServer/LanguageServer.cs) instance, which was implemented by the PowerFx.LSP package.   
1. the `LanguageServer` class is sealed and ultimately calls back on the `RecalcEngine` instance to access symbols. It gets this instance via a [IPowerFxScopeFactory](https://github.com/microsoft/Power-Fx/blob/main/src/libraries/Microsoft.PowerFx.LanguageServerProtocol/LanguageServer/IPowerFxScopeFactory.cs) passed to the ctor. The `IPowerFxScopeFactory` maps the getDocumentUriAsync value back to a  [IPowerFxScope](https://github.com/microsoft/Power-Fx/blob/main/src/libraries/Microsoft.PowerFx.Core/Public/IPowerFxScope.cs) instance. This scope instance includes the `RecalcEngine` instance and captures the context from getDocumentUriAsync. In this sample, that context is the json state variables. 
1. The `LanguageServer` calls methods on `IPowerFxScope` to do intellisense and returns the results back over the network channel according to the LSP. 
1. The client control renders the results. 


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

