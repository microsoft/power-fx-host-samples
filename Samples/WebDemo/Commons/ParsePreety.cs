using System.Linq;
using System.Text;
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
        private int _indent;

        public ParsePreety()
        {
            _indent = 0;
        }

        public static string PrettyPrint(TexlNode node)
        {
            var pretty = new ParsePreety();
            var sb = new StringBuilder();
            return sb.Append(node.Accept(pretty, Precedence.None).ToString()).ToString();
        }

        public override StringBuilder Visit(ErrorNode node, Precedence context)
        {
            return new StringBuilder().Append("<error>");
        }

        public override StringBuilder Visit(BlankNode node, Precedence context)
        {
            return new StringBuilder();
        }

        public override StringBuilder Visit(BoolLitNode node, Precedence context)
        {
            return new StringBuilder(node.Value ? " true " : "false")
                .Append(" (" + node.Kind.ToString() + ")");
        }

        public override StringBuilder Visit(StrLitNode node, Precedence context)
        {
            var result = new StringBuilder()
                .Append("\"")
                .Append(node.Value)
                .Append(" (" + node.Kind.ToString() + ")")
                .Append("\"");
            return result;
        }

        public override StringBuilder Visit(NumLitNode node, Precedence context)
        {
            var result = new StringBuilder();
            result.Append(node.ActualNumValue.ToString())
                .Append(" (" + node.Kind.ToString() + ")");
            return result;
        }

        public override StringBuilder Visit(FirstNameNode node, Precedence context)
        {
            return new StringBuilder()
                .Append("\"" + node.Ident.ToString().Split(".").Last() + "\" : ")
                .Append("\"" + node.ToString() + "\"");
        }

        public override StringBuilder Visit(ParentNode node, Precedence context)
        {
            return new StringBuilder(node.ToString());
        }

        public override StringBuilder Visit(SelfNode node, Precedence context)
        {
            return new StringBuilder(node.Kind.ToString());
        }

        public override StringBuilder Visit(StrInterpNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder()
                .Append(AddIndent())
                .Append("$\"");

            var children = node.ChildNodes;

            foreach (var child in children)
            {
                if (child.Kind == NodeKind.StrLit)
                {
                    result.Append((child as StrLitNode).Value);
                }
                else
                {
                    result.Append("{")
                        .Append(child.Accept(this, Precedence.None))
                        .Append("}");
                }
            }
            return result;
        }

        public override StringBuilder Visit(DottedNameNode node, Precedence parentPrecedence)
        {
            _indent++;
            var result = new StringBuilder()
                .Append(AddIndent())
                .Append("\"" + node.Kind.ToString() + "\"")
                .Append("{");

            _indent++;

            result.Append(AddIndent())
                .Append("\"Left\": {" + node.Left.Accept(this, Precedence.Primary) + "}")
                .Append(AddIndent())
                .Append("\"Right\": {")
                .Append("\"" + node.Right.ToString().Split(".").Last() + "\" : ")
                .Append("\"" + node.Right.Name + "\"")
                .Append("}");

            _indent--;

            result.Append("}");

            return ApplyPrecedence(parentPrecedence, Precedence.Primary, result);
        }

        public override StringBuilder Visit(UnaryOpNode node, Precedence parentPrecedence)
        {
            var child = node.Child.Accept(this, Precedence.PrefixUnary);

            var result = new StringBuilder();
            switch (node.Op)
            {
                case UnaryOp.Not:
                    result.Append(UnaryOp.Not.ToString() + " ")
                        .Append(child);
                    break;

                case UnaryOp.Minus:
                    result.Append("-")
                        .Append(child);
                    break;

                case UnaryOp.Percent:
                    result.Append("%")
                        .Append(child);
                    break;

                default:
                    result.Append("<error> ")
                        .Append(child);
                    break;
            }

            return ApplyPrecedence(parentPrecedence, Precedence.PrefixUnary, result);
        }

        public override StringBuilder Visit(BinaryOpNode node, Precedence parentPrecedence)
        {
            switch (node.Op)
            {
                case BinaryOp.Or:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Or, node.Left, node.Right);

                case BinaryOp.And:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.And, node.Left, node.Right);

                case BinaryOp.Concat:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Concat, node.Left, node.Right);

                case BinaryOp.Add:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Add, node.Left, node.Right);

                case BinaryOp.Mul:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Mul, node.Left, node.Right);

                case BinaryOp.Div:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Mul, node.Left, node.Right);

                case BinaryOp.In:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.In, node.Left, node.Right);

                case BinaryOp.Exactin:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.In, node.Left, node.Right);

                case BinaryOp.Power:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Power, Precedence.PrefixUnary, node.Left, node.Right);

                case BinaryOp.Error:
                    return PrettyBinary(" <error> ", parentPrecedence, Precedence.Error, node.Left, node.Right);

                case BinaryOp.Equal:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Compare, node.Left, node.Right);

                case BinaryOp.NotEqual:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Compare, node.Left, node.Right);

                case BinaryOp.Less:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Compare, node.Left, node.Right);

                case BinaryOp.LessEqual:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Compare, node.Left, node.Right);

                case BinaryOp.Greater:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Compare, node.Left, node.Right);

                case BinaryOp.GreaterEqual:
                    return PrettyBinary(node.Op.ToString(), parentPrecedence, Precedence.Compare, node.Left, node.Right);

                default:
                    return PrettyBinary(" <error> ", parentPrecedence, Precedence.Atomic + 1, node.Left, node.Right);
            }
        }

        public override StringBuilder Visit(VariadicOpNode node, Precedence context)
        {
            var result = new StringBuilder();
            switch (node.Op)
            {
                case VariadicOp.Chain:
                    var count = node.Count;
                    for (var i = 0; i < count; i++)
                    {
                        result.Append(node.ChildNodes[i].Accept(this, Precedence.None));
                        if (i != count - 1)
                        {
                            result.Append(";");
                        }
                    }
                    return result;

                default:
                    return result.Append("<error>");
            }
        }

        public override StringBuilder Visit(CallNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();
            _indent++;
            result.Append("\"" + node.Kind.ToString() + "\" : ")
                .Append(node.Head.Name)
                .Append("(")
                .Append(AddIndent())
                .Append(node.Args.Accept(this, Precedence.Primary).ToString())
                .Append(AddIndent())
                .Append(")");
            _indent--;
            return ApplyPrecedence(parentPrecedence, Precedence.Primary, result);
        }

        // Verify this
        public override StringBuilder Visit(ListNode node, Precedence context)
        {
            var result = new StringBuilder();

            for (var i = 0; i < node.ChildNodes.Count; i++)
            {
                result.Append(node.ChildNodes[i].Accept(this, Precedence.None));
                if (i != node.ChildNodes.Count - 1)
                {
                    result.Append(", ");
                }
            }

            return result;
        }

        public override StringBuilder Visit(RecordNode node, Precedence parentPrecedence)
        {
            var listSep = ", ";
            var result = new StringBuilder();
            result.Append("{");
            for (var i = 0; i < node.ChildNodes.Count(); ++i)
            {
                result.Append("\"")
                    .Append(node.Ids[i].Name)
                    .Append(" (" + node.Ids[i].ToString().Split(".").Last() + ")")
                    .Append("\"")
                    .Append(" : ")
                    .Append(node.ChildNodes[i].Accept(this, Precedence.SingleExpr));
                if (i != node.ChildNodes.Count() - 1)
                {
                    result.Append(listSep);
                }
            }
            result.Append("}");

            return ApplyPrecedence(parentPrecedence, Precedence.SingleExpr, result);
        }

        public override StringBuilder Visit(TableNode node, Precedence parentPrecedence)
        {
            var result = new StringBuilder();
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

            return ApplyPrecedence(parentPrecedence, Precedence.SingleExpr, result);
        }

        public override StringBuilder Visit(AsNode node, Precedence parentPrecedence)
        {
            return ApplyPrecedence(
                parentPrecedence,
                Precedence.As,
                node.Left.Accept(this, Precedence.As)
                    .Append(SpacedOper(node.Kind.ToString()))
                    .Append(node.Right.ToString()));
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
        private StringBuilder PrettyBinary(string strOp, Precedence parentPrecedence, Precedence precLeft, TexlNode left, TexlNode right)
        {
            return PrettyBinary(strOp, parentPrecedence, precLeft, precLeft + 1, left, right);
        }

        private StringBuilder PrettyBinary(string strOp, Precedence parentPrecedence, Precedence precLeft, Precedence precRight, TexlNode left, TexlNode right)
        {
            _indent++;
            var result = new StringBuilder()
                .Append(AddIndent())
                .Append("\"Binary Operation\":")
                .Append("{");

            _indent++;

            result.Append(AddIndent())
                .Append("\"type\" : " + "\"" + strOp + "\"")
                .Append(AddIndent())
                .Append("\"Left\": {" + left.Accept(this, precLeft).ToString() + "}")
                .Append(AddIndent())
                .Append("\"Right\": {" + right.Accept(this, precRight).ToString() + "}");

            _indent--;

            result.Append(AddIndent())
                .Append("}");

            _indent--;

            return result;
        }

        // Adds a new line and a tabs for indentation
        private string AddIndent()
        {
            var result = new StringBuilder("\n");
            for (var i = 0; i < _indent; i++)
            {
                result.Append("\t");
            }
            return result.ToString();
        }

        private string SpacedOper(string op)
        {
            return " " + op + " ";
        }
    }
}
