// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.PowerFx;
using Microsoft.PowerFx.Core;
using Microsoft.PowerFx.Core.Public;
using Microsoft.PowerFx.Intellisense;
using System;
using System.Web;

namespace PowerFxService.Model
{
    public class PowerFxScopeFactory : IPowerFxScopeFactory
    {
        // Ensure that we're getting the same engine used by intellisense (LSP) and evaluation.
        public RecalcEngine GetEngine()
        {
            // If the engine requires additional symbols to load, server
            // should find a way to safely cache it. 
            var engine = new RecalcEngine();

            return engine;
        }

        // A scope wraps the engine and provides parameters used for intellisense.
        public RecalcEngineScope GetScope(string contextJson)
        {
            var engine = GetEngine();

            return RecalcEngineScope.FromJson(engine, contextJson);
        }

        // Uri is passed in from the front-end and specifies which formula bar. 
        // Returns an object that provides intellisense support. 
        public IPowerFxScope GetOrCreateInstance(string documentUri)
        {
            // The host could pass in additional information in the Uri here to help 
            // initialize a formula bar or distinguish between multiple formula bars. 

            // The context is additional symbols passed by the host.             
            var uriObj = new Uri(documentUri);
            var contextJson = HttpUtility.ParseQueryString(uriObj.Query).Get("context");
            if (contextJson == null)
            {
                contextJson = "{}";
            }

            var scope = GetScope(contextJson);
            return scope;
        }
    }
}
