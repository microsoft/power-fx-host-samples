// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;
using PowerFxService.Model;
using System;

namespace PowerFxService.Controllers
{
    // Called by the demo page to evaluate an expression. 
    [ApiController]
    [Route("eval")]
    public class EvaluationController : ControllerBase
    {
        private readonly ILogger<EvaluationController> _logger;
        
        public EvaluationController(ILogger<EvaluationController> logger)
        {
            _logger = logger;
        }

        // Passed  in from demo page to do an evaluation. 
        public class RequestBody
        {
            // additional symbols passed in as a json object. 
            public string context { get; set; }

            // the user's Power Fx expression to be evaluated 
            public string expression { get; set; }
        }

        [HttpPost]
        public IActionResult HandleRequest([FromBody] RequestBody body)
        {
            try
            {
                var engine = new PowerFxScopeFactory().GetEngine();
                
                var parameters = (RecordValue) FormulaValue.FromJson(body.context);
                
                // TODO - this should be eval Async and catch timeouts. 
                if (parameters == null)
                {
                    parameters = RecordValue.Empty();
                }
                var check = engine.Check(body.expression, parameters.Type, options:null);
                check.ThrowOnErrors();
                var result =  check.Expression.EvalAsync(parameters, cancel: CancellationToken.None).Result;

                var resultString = PowerFxHelper.TestToString(result);

                return StatusCode(200, new { result = resultString });
            }
            catch (Exception ex)
            {
                return StatusCode(200, new { error = ex.Message });
            }
        }
    }
}
