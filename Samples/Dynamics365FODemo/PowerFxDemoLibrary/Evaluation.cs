using Microsoft.PowerFx;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerFx.Types;
using System;
using System.Collections.Generic;

namespace PowerFxDemoLibrary
{
    public class Evaluation
    {
        public class Response
        {
            public string output { get; set; }
            public string error { get; set; }
            public bool   errorThrown { get; set; }
        }

        private class ThrowErrorFunction : ReflectionFunction
        {
            private string errorMessage = "";

            public ThrowErrorFunction(): base("ThrowError", FormulaType.Blank, new[] { FormulaType.String }) { }

            public BlankValue Execute(StringValue x)
            {
                addError(x.Value);
                return FormulaValue.NewBlank();
            }

            public void addError(string error)
            {
                if (error != "")
                {
                    errorMessage += errorMessage + "\n" + error;
                }
                else
                {
                    errorMessage += errorMessage + "\n";
                }
            }

            public string getErrorMessage()
            {
                return errorMessage.TrimStart('\n').TrimEnd('\n');
            }

            public bool isFailed()
            {
                return errorMessage != "" ? true : false;
            }

        }

        private class ThrowErrorFunctionEmtpy : ReflectionFunction
        {
            public ThrowErrorFunction function;
            public ThrowErrorFunctionEmtpy(ThrowErrorFunction _function) : base("ThrowError", FormulaType.Blank) 
            {
                function = _function;
            }

            public BlankValue Execute()
            {
                function.addError("");
                return FormulaValue.NewBlank();
            }
        }

        public Response EvaluateInput(string context, string expression)
        {
            Response response = new Response();
            IReadOnlyList<Token> tokens = null;
            CheckResult check = null;

            ThrowErrorFunction errorFunction = new ThrowErrorFunction();
            ThrowErrorFunctionEmtpy errorFunctionEmpty = new ThrowErrorFunctionEmtpy(errorFunction);

            response.output = "";
            response.error = "";
            response.errorThrown = false;
            try
            {
                var config = new PowerFxConfig();
                config.AddFunction(errorFunction);
                config.AddFunction(errorFunctionEmpty);
                var engine = new RecalcEngine(config);

                var parameters = (RecordValue)FormulaValueJSON.FromJson(context);

                if (parameters == null)
                {
                    parameters = RecordValue.Empty();
                }

                tokens = engine.Tokenize(expression);
                check = engine.Check(expression, parameters.Type, options: null);
                check.ThrowOnErrors();
                var eval = check.GetEvaluator();
                var result = eval.Eval(parameters);

                response.output = HelperClass.TestToString(result);
            }
            catch(Exception ex)
            {
                errorFunction.addError(ex.Message);
            }
            finally
            {
                response.errorThrown = errorFunction.isFailed();
                response.error = errorFunction.getErrorMessage();
            }

            return response;

        }

    }

}
