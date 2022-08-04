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
        public static TimeSpan timeout = TimeSpan.FromSeconds(2);

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
            cts.CancelAfter(timeout);
            try
            {
                var engine = new PowerFxScopeFactory().GetEngine();

                var parameters = (RecordValue)FormulaValue.FromJson(body.context);

                if (parameters == null)
                {
                    parameters = RecordValue.Empty();
                }

                var result = await EvalAsync(engine, body.expression, cts.Token, parameters, options: null);
                var resultString = PowerFxHelper.TestToString(result);

                return StatusCode(200, new { result = resultString });
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

        private async Task<FormulaValue> EvalAsync(IPowerFxEngine engine, string expressionText, CancellationToken cancel, RecordValue parameters = null, ParserOptions options = null)
        {
            if (parameters == null)
            {
                parameters = RecordValue.Empty();
            }
            var check = engine.Check(expressionText, parameters.Type, options);
            check.ThrowOnErrors();
            return await check.Expression.EvalAsync(parameters, cancel);
        }
    }
}
