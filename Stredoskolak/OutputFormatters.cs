using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    public static class OutputFormatters
    {
        static Dictionary<string, IOutputFormatterFactory> formats = new Dictionary<string, IOutputFormatterFactory>()
        {
            { "dummy", new DummyFormatterFactory() },
            { "default", new DefaultFormatterFactory() }
        };

        public static IExpressionVisitor<string> GetFormatter(string name)
        {
            if (formats.ContainsKey(name)) return formats[name].GetFormater();
            else return formats["default"].GetFormater();
        }
    }

    public interface IOutputFormatterFactory
    {
        IExpressionVisitor<string> GetFormater();
    }

    public class DefaultFormatterFactory : IOutputFormatterFactory
    {
        public IExpressionVisitor<string> GetFormater()
        {
            return new DefaultFormatter();
        }
    }
    public class DefaultFormatter : IExpressionVisitor<string>
    {
        public string Visit(OpRational number) => number.ToString();
        public string Visit(OpVariable variable) => variable.Name;
        public string Visit(OpAdditiveInverse op) => "-" + op.Operand.Accept(this);
        public string Visit(OpMultiplicativeInverse op) => "/" + op.Operand.Accept(this);

        public string Visit(OpSum op)
        {
            if (op.Operands.Count == 0) return "0";
            else if (op.Operands.Count == 1) return op.Operands[0].Accept(this);

            string result = "(";
            result += op.Operands[0].Accept(this);

            for (int i = 1; i < op.Operands.Count; i++)
            {
                result += '+';
                result += op.Operands[i].Accept(this);
            }

            return result + ")";
        }
        public string Visit(OpProduct op)
        {
            if (op.Operands.Count == 0) return "1";
            else if (op.Operands.Count == 1) return op.Operands[0].Accept(this);

            string result = "(";
            result += op.Operands[0].Accept(this);

            for (int i = 1; i < op.Operands.Count; i++)
            {
                result += '*';
                result += op.Operands[i].Accept(this);
            }

            return result + ")";
        }

        public string Visit(OpLogarhithm op) => $"log({op.OperandBase.Accept(this)},{op.OperandArgument.Accept(this)})";
        public string Visit(OpPower op) => $"pow({op.OperandBase.Accept(this)},{op.OperandExponent.Accept(this)})";
        public string Visit(OpRoot op) => $"root({op.OperandExponent.Accept(this)},{op.OperandBase.Accept(this)})";

        public string Visit(OpSin op) => $"sin({op.Operand.Accept(this)})";
        public string Visit(OpCos op) => $"cos({op.Operand.Accept(this)})";
        public string Visit(OpTg op) => $"tg({op.Operand.Accept(this)})";
        public string Visit(OpCotg op) => $"cotg({op.Operand.Accept(this)})";
        public string Visit(OpSec op) => $"sec({op.Operand.Accept(this)})";
        public string Visit(OpCsc op) => $"csc({op.Operand.Accept(this)})";

        public string Visit(OpArcSin op) => $"arcsin({op.Operand.Accept(this)})";
        public string Visit(OpArcCos op) => $"arccos({op.Operand.Accept(this)})";
        public string Visit(OpArcTg op) => $"arctg({op.Operand.Accept(this)})";
        public string Visit(OpArcCotg op) => $"arccotg({op.Operand.Accept(this)})";
        public string Visit(OpArcSec op) => $"arcsec({op.Operand.Accept(this)})";
        public string Visit(OpArcCsc op) => $"arccsc({op.Operand.Accept(this)})";

        public string Visit(OpPi op) => "pi";
        public string Visit(OpE op) => "e";
        public string Visit(OpParam op) => op.Name;
    }

    // Solely used to demostrate extensibility
    public class DummyFormatterFactory : IOutputFormatterFactory
    {
        public IExpressionVisitor<string> GetFormater()
        {
            return new DummyFormatter();
        }
    }
    public class DummyFormatter : IExpressionVisitor<string>
    {
        public string Visit(OpRational number) => "Num";
        public string Visit(OpVariable variable) => "Var";
        public string Visit(OpAdditiveInverse op) => "AddInv";
        public string Visit(OpMultiplicativeInverse op) => "MulInv";

        public string Visit(OpSum op) => "Sum";
        public string Visit(OpProduct op) => "Prod";

        public string Visit(OpLogarhithm op) => "Log";
        public string Visit(OpPower op) => "Pow";
        public string Visit(OpRoot op) => "Root";

        public string Visit(OpSin op) => "Sin";
        public string Visit(OpCos op) => "Cos";
        public string Visit(OpTg op) => "Tg";
        public string Visit(OpCotg op) => "Cotg";
        public string Visit(OpSec op) => "Sec";
        public string Visit(OpCsc op) => "Csc";

        public string Visit(OpArcSin op) => "ArcSin";
        public string Visit(OpArcCos op) => "ArcCos";
        public string Visit(OpArcTg op) => "ArcTg";
        public string Visit(OpArcCotg op) => "ArcCotg";
        public string Visit(OpArcSec op) => "ArcSec";
        public string Visit(OpArcCsc op) => "ArcCsc";
        public string Visit(OpPi op) => "Pi";
        public string Visit(OpE op) => "E";
        public string Visit(OpParam op) => "Param";
    }
}
