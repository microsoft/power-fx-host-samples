using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.PowerFx.Syntax;

namespace WebDemo.Commons
{
    internal enum Precedence : byte
    {
        None,
        SingleExpr,
        Or,
        And,
        In,
        Compare,
        Concat,
        Add,
        Mul,
        Error,
        As,
        PrefixUnary,
        Power,
        PostfixUnary,
        Primary,
        Atomic,
    }

    internal class ParsePreety : TexlFunctionalVisitor<StringBuilder, Precedence>
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ParsePreety()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
        }

        public static string PrettyPrint(TexlNode node)
        {
            var pretty = new ParsePreety();
            var result = new StringBuilder();
            result.Append(node.Accept(pretty, Precedence.None).ToString());
            return result.ToString();
        }

        public override StringBuilder Visit(ErrorNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(BlankNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(BoolLitNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(StrLitNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(NumLitNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(FirstNameNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(ParentNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(SelfNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, options: _jsonSerializerOptions));
            return result;
        }

        public override StringBuilder Visit(StrInterpNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();
            result.Append("{");
            result.Append($"\"Kind\" : \"{node.Kind.ToString()}\" ,");

            var children = node.ChildNodes;
            var resultList = new List<string>();
            foreach (var child in children)
            {
                if (child.Kind == NodeKind.StrLit)
                {
                    resultList.Add(JsonSerializer.Serialize(child as StrLitNode, options: _jsonSerializerOptions));
                }
                else
                {
                    resultList.Add(child.Accept(this, Precedence.None).ToString());
                }
            }
            result.Append("\"children\" : [" + string.Join(", ", resultList) + "]");
            result.Append("}");
            return result;
        }

        public override StringBuilder Visit(DottedNameNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();
            result.Append("{");

            result.Append("\"Kind\" : \"{node.Kind.ToString()}\" ,")
            .Append("\"Left\": " + node.Left.Accept(this, Precedence.Primary))
            .Append(",")
            .Append("\"Right\": ")
            .Append(JsonSerializer.Serialize(node.Right, _jsonSerializerOptions));

            result.Append("}");

            return ApplyPrecedence(parentPrecedence, Precedence.Primary, result);
        }

        public override StringBuilder Visit(UnaryOpNode node, Precedence parentPrecedence)
        {
            var child = node.Child.Accept(this, Precedence.PrefixUnary);
            var result = new StringBuilder();
            result.Append("{");
            result.Append($"\"Kind\" : \"{node.Kind.ToString()}\" ,");
            result.Append("\"Op\" : " + "\"" + node.Op.ToString() + "\"");
            result.Append(",");
            result.Append("\"Child\" :" + child);
            result.Append("}");
            return ApplyPrecedence(parentPrecedence, Precedence.PrefixUnary, result);
        }

        public override StringBuilder Visit(BinaryOpNode node, Precedence parentPrecedence)
        {
            switch (node.Op)
            {
                case BinaryOp.Or:
                    return PrettyBinary(node, parentPrecedence, Precedence.Or);

                case BinaryOp.And:
                    return PrettyBinary(node, parentPrecedence, Precedence.And);

                case BinaryOp.Concat:
                    return PrettyBinary(node, parentPrecedence, Precedence.Concat);

                case BinaryOp.Add:
                    return PrettyBinary(node, parentPrecedence, Precedence.Add);

                case BinaryOp.Mul:
                    return PrettyBinary(node, parentPrecedence, Precedence.Mul);

                case BinaryOp.Div:
                    return PrettyBinary(node, parentPrecedence, Precedence.Mul);

                case BinaryOp.In:
                    return PrettyBinary(node, parentPrecedence, Precedence.In);

                case BinaryOp.Exactin:
                    return PrettyBinary(node, parentPrecedence, Precedence.In);

                case BinaryOp.Power:
                    return PrettyBinary(node, parentPrecedence, Precedence.Power);

                case BinaryOp.Error:
                    return PrettyBinary(node, parentPrecedence, Precedence.Error);

                case BinaryOp.Equal:
                    return PrettyBinary(node, parentPrecedence, Precedence.Compare);

                case BinaryOp.NotEqual:
                    return PrettyBinary(node, parentPrecedence, Precedence.Compare);

                case BinaryOp.Less:
                    return PrettyBinary(node, parentPrecedence, Precedence.Compare);

                case BinaryOp.LessEqual:
                    return PrettyBinary(node, parentPrecedence, Precedence.Compare);

                case BinaryOp.Greater:
                    return PrettyBinary(node, parentPrecedence, Precedence.Compare);

                case BinaryOp.GreaterEqual:
                    return PrettyBinary(node, parentPrecedence, Precedence.Compare);

                default:
                    return PrettyBinary(node, parentPrecedence, Precedence.Atomic + 1);
            }
        }

        public override StringBuilder Visit(VariadicOpNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append("{");
            result.Append($"\"Kind\" : \"{node.Kind.ToString()}\" ,");

            result.Append("\"children\" : ");
            result.Append("[");
            switch (node.Op)
            {
                case VariadicOp.Chain:
                    var count = node.Count;
                    for (var i = 0; i < count; i++)
                    {
                        result.Append(node.ChildNodes[i].Accept(this, Precedence.None));
                        if (i != count - 1)
                        {
                            result.Append(",");
                        }
                    }
                    break;
                default:
                        result.Append("<error>");
                    break;
            }
            result.Append("]");
            result.Append("}");
            return result;
        }

        public override StringBuilder Visit(CallNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();
            result.Append(JsonSerializer.Serialize(node, _jsonSerializerOptions));

            return ApplyPrecedence(parentPrecedence, Precedence.Primary, result);
        }

        // Verify this
        public override StringBuilder Visit(ListNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append("{");
            result.Append("\"Kind\" :");
            result.Append("\"" + node.Kind.ToString() + "\" , ");
            result.Append("\"children\" : ");
            result.Append("[");
            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                result.Append(node.ChildNodes[i].Accept(this, Precedence.None));
                if (i != node.ChildNodes.Count - 1)
                {
                    result.Append(", ");
                }
            }
            result.Append("]");
            result.Append("}");
            return result;
        }

        public override StringBuilder Visit(RecordNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();
            result.Append("{");
            result.Append("\"Kind\" :");
            result.Append("\"" + node.Kind.ToString() + "\" , ");
            result.Append("\"children\" : ");
            result.Append("[");
            for (var i = 0; i < node.ChildNodes.Count(); ++i)
            {
                result.Append(node.ChildNodes[i].Accept(this, Precedence.SingleExpr));
                if (i != node.ChildNodes.Count() - 1)
                {
                    result.Append(", ");
                }
            }
            result = result.Append("]");
            result.Append("}");

            return ApplyPrecedence(parentPrecedence, Precedence.SingleExpr, result);
        }

        public override StringBuilder Visit(TableNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();
            result.Append("{");
            result.Append("\"Kind\" :");
            result.Append("\"" + node.Kind.ToString() + "\" , ");

            result.Append("\"children\" : ");
            result.Append("[");
            var len = node.ChildNodes.Count();
            for (var i = 0; i < len; i++)
            {
                result.Append(node.ChildNodes[i].Accept(this, Precedence.SingleExpr));
                if (i != len - 1)
                {
                    result.Append(", ");
                }
            }
            result.Append("]");
            result.Append("}");
            return ApplyPrecedence(parentPrecedence, Precedence.SingleExpr, result);
        }

        public override StringBuilder Visit(AsNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();

            result.Append("{")
            .Append("\"Kind\" :")
            .Append("\"" + node.Kind.ToString() + "\" , ")
            .Append("\"Left\" :")
            .Append(node.Left.Accept(this, Precedence.As))
            .Append(",")
            .Append("\"Right\" :")
            .Append(JsonSerializer.Serialize(node.Right, _jsonSerializerOptions))
            .Append("}");

            return result;
        }

        private StringBuilder ApplyPrecedence(Precedence parentPrecedence, Precedence precedence, StringBuilder strings)
        {
            if (parentPrecedence > precedence)
            {
                var result = new StringBuilder()
                    .Append("(")
                    .Append(strings)
                    .Append(')');
                return result;
            }

            return strings;
        }

        // For left associative operators: precRight == precLeft + 1.
        private StringBuilder PrettyBinary(BinaryOpNode node, Precedence precLeft)
        {
            return PrettyBinary(node, precLeft, precLeft + 1);
        }

        private StringBuilder PrettyBinary(BinaryOpNode node, Precedence precLeft, Precedence precRight)
        {
            var result = new StringBuilder();

            result.Append("{")
            .Append("\"Kind\" :")
            .Append("\"" + node.Kind.ToString() + "\" , ")
            .Append("\"Op\" : ")
            .Append("\"" + node.Op.ToString() + "\"")
            .Append(",")
            .Append("\"Left\" :")
            .Append(node.Left.Accept(this, precLeft).ToString())
            .Append(",")
            .Append("\"Right\" :")
            .Append(node.Right.Accept(this, precRight).ToString())
            .Append("}");

            return result;
        }

        private string SpacedOper(string op)
        {
            return " " + op + " ";
        }
    }
}
