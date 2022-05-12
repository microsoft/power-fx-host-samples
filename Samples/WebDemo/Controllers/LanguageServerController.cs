// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerFx.LanguageServerProtocol;
using PowerFxService.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace PowerFxService.Controllers
{
    /// <summary>
    /// This controller services Language Service Protocol requests from the formula bar. 
    /// It just creates a network channel between the formula bar control and the <see cref="LanguageServer"/> object. 
    /// </summary>
    [ApiController]
    [Route("lsp")]
    public class LanguageServerController : ControllerBase
    {
        private readonly ILogger<LanguageServerController> _logger;

        public LanguageServerController(ILogger<LanguageServerController> logger)
        {
            _logger = logger;
        }

        // Passed in a json payload which is just passed directly to LanguageServer. 
        [HttpPost]
        public IActionResult HandleRequest([FromBody] JsonElement body)
        {
            // The scope factory.
            // This lets the LanguageServer object get the symbols in the RecalcEngine to use for intellisense.
            var scopeFactory = new PowerFxScopeFactory();

            var sendToClientData = new List<string>();
            var languageServer = new LanguageServer((string data) => sendToClientData.Add(data), scopeFactory);

            try
            {
                languageServer.OnDataReceived(body.ToString());
                return StatusCode(200, sendToClientData.ToArray());
            }
            catch (Exception ex)
            {
                return StatusCode(400, new { error = ex.Message });
            }
        }
    }
}
