using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    public class EquationOperation
    {
        public EquationOperation(EquationOperationType type, Expression parameter, Expression newValueOfOneSide)
        {
            OperationType = type;
            Parameter = parameter;
            NewValueOfOneSide = newValueOfOneSide;
        }

        public EquationOperationType OperationType;
        public Expression Parameter;

        public Expression NewValueOfOneSide;

        public Expression ApplyToExpression(Expression ex)
        {
            switch (OperationType)
            {
                case EquationOperationType.plus:
                    return new OpSum(ex, Parameter);
                case EquationOperationType.minus:
                    return new OpSum(ex, new OpAdditiveInverse(Parameter));
                case EquationOperationType.multiply:
                    return new OpProduct(ex, Parameter);
                case EquationOperationType.divide:
                    return new OpProduct(ex, new OpMultiplicativeInverse(Parameter));
                case EquationOperationType.root:
                    return new OpRoot(Parameter, ex);
                case EquationOperationType.power:
                    return new OpPower(ex, Parameter);
                case EquationOperationType.powerFromLeft:
                    return new OpPower(Parameter, ex);
                case EquationOperationType.logarithm:
                    return new OpLogarhithm(Parameter, ex);
                case EquationOperationType.flipSign:
                    return new OpAdditiveInverse(Parameter);
                case EquationOperationType.invert:
                    return new OpMultiplicativeInverse(Parameter);
                case EquationOperationType.none:
                    break;
                case EquationOperationType.arcsin:
                    return new OpArcSin(ex);
                case EquationOperationType.arccos:
                    return new OpArcCos(ex);
                case EquationOperationType.arctg:
                    return new OpArcTg(ex);
                case EquationOperationType.arccotg:
                    return new OpArcCotg(ex);
                case EquationOperationType.arcsec:
                    return new OpArcSec(ex);
                case EquationOperationType.arccsc:
                    return new OpArcCsc(ex);
                case EquationOperationType.sin:
                    return new OpSin(ex);
                case EquationOperationType.cos:
                    return new OpCos(ex);
                case EquationOperationType.tg:
                    return new OpTg(ex);
                case EquationOperationType.cotg:
                    return new OpCotg(ex);
                case EquationOperationType.sec:
                    return new OpSec(ex);
                case EquationOperationType.csc:
                    return new OpCsc(ex);
            }
            return ex;
        }

        public override string ToString()
        {
            switch (OperationType)
            {
                case EquationOperationType.none: return "Do nothing";
                case EquationOperationType.plus: return $"|+({Parameter})";
                case EquationOperationType.minus: return $"|-({Parameter})";
                case EquationOperationType.multiply: return $"|*({Parameter})";
                case EquationOperationType.divide: return $"|/({Parameter})";
                case EquationOperationType.root: return $"|root({Parameter},[])";
                case EquationOperationType.power: return $"|pow([],{Parameter})";
                case EquationOperationType.powerFromLeft: return $"|pow({Parameter},[])";
                case EquationOperationType.logarithm: return $"|log({Parameter},[])";
                case EquationOperationType.flipSign: return $"|-[]";
                case EquationOperationType.invert: return $"|/[]";
                case EquationOperationType.evalPolynomial: return $"Evaluate polynomial";
                case EquationOperationType.evalTrigTable: return $"Evaluate inverse trigonometric function";
                case EquationOperationType.swapSides: return $"Swap sides";
                case EquationOperationType.sin: return $"sin([])";
                case EquationOperationType.cos: return $"cos([])";
                case EquationOperationType.tg: return $"tg([])";
                case EquationOperationType.cotg: return $"cotg([])";
                case EquationOperationType.sec: return $"sec([])";
                case EquationOperationType.csc: return $"csc([])";
                case EquationOperationType.arcsin: return $"arcsin([])";
                case EquationOperationType.arccos: return $"arccos([])";
                case EquationOperationType.arctg: return $"arctg([])";
                case EquationOperationType.arccotg: return $"arccotg([])";
                case EquationOperationType.arcsec: return $"arcsec([])";
                case EquationOperationType.arccsc: return $"arccsc([])";
                default: return "This shouldn't happen.";
            }
        }
    }
    public enum EquationOperationType
    {
        none, plus, minus, multiply, divide, root, power, powerFromLeft, logarithm, flipSign, invert, evalPolynomial, evalTrigTable,
        arcsin, arccos, arctg, arccotg, arcsec, arccsc, sin, cos, tg, cotg, sec, csc, swapSides
    }
}
