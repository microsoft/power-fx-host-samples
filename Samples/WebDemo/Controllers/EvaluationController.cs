// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.PowerFx;
using Microsoft.PowerFx.Core.Utils;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerFx.Types;
using PowerFxService.Model;
using WebDemo.Commons;

namespace PowerFxService.Controllers
{
    // Called by the demo page to evaluate an expression.
    [ApiController]
    [Route("eval")]
    public class EvaluationController : ControllerBase
    {
        private readonly ILogger<EvaluationController> _logger;
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(2);
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public EvaluationController(ILogger<EvaluationController> logger)
        {
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                    new NodeConverter<TexlNode>(),
                    new NodeConverter<VariadicOpNode>(),
                    new NodeConverter<ListNode>(),
                    new NodeConverter<CallNode>(),
                    new NodeConverter<Identifier>(),
                    new NodeConverter<DName>()
                }
            };
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
            IReadOnlyList<Token> tokens = null;
            CheckResult check = null;
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

                tokens = engine.Tokenize(body.expression);
                check = engine.Check(body.expression, parameters.Type, options: null);
                check.ThrowOnErrors();
                var eval = check.GetEvaluator();                
                var result = await eval.EvalAsync(cts.Token, parameters);
                var resultString = PowerFxHelper.TestToString(result);
                return StatusCode(200, new
                {
                    result = resultString,
                    tokens = tokens,
                    parse = JsonSerializer.Serialize(check.Parse.Root, _jsonSerializerOptions)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(200, new
                {
                    error = ex.Message,
                    tokens = tokens,
                    parse = JsonSerializer.Serialize(check.Parse.Root, _jsonSerializerOptions)
                });
            }
            finally
            {
                cts.Dispose();
            }
        }
    }
}
