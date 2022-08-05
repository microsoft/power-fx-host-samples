// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Types;
using PowerFxService.Model;

namespace PowerFxService.Controllers
{
    // Called by the demo page to evaluate an expression.
    [ApiController]
    [Route("eval")]
    public class EvaluationController : ControllerBase
    {
        private readonly ILogger<EvaluationController> _logger;
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(2);

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
        public async Task<IActionResult> HandleRequestAsync([FromBody] RequestBody body)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(_timeout);
            try
            {
                var engine = new PowerFxScopeFactory().GetEngine();

                var parameters = (RecordValue)FormulaValue.FromJson(body.context);

                if (parameters == null)
                {
                    parameters = RecordValue.Empty();
                }
                var check = engine.Check(body.expression, parameters.Type, options:null);
                check.ThrowOnErrors();
                var result = await check.Expression.EvalAsync(parameters, cts.Token);
                var tokens = engine.Tokenize(body.expression);
                var resultString = PowerFxHelper.TestToString(result);

                return StatusCode(200, new { result = resultString, tokens = tokens, parse = check.Parse });
            }
            catch (Exception ex)
            {
                return StatusCode(200, new { error = ex.Message });
            }
            finally
            {
                cts.Dispose();
            }
        }
    }
}
