using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Stredoskolak
{
    public abstract class Expression
    {
        public abstract T Accept<T>(IExpressionVisitor<T> visitor);
        public abstract bool IsEquivalent(Expression ex);
    }
    public static class ExpressionHelper
    {
        public static Expression Clone(this Expression e) => e.Accept(new ExpressionCopier());
        public static Expression SimplifyingCopy(this Expression e) => e.Accept(new ExpressionSimplifier()).Item2;
        public static bool IsRealNumber(this Expression e) => e.Accept(new RealNumberExpressionChecker());
        public static HashSet<string> ListVariables(this Expression e) => e.Accept(new VariableLister());
        public static Expression SubstitutingCopy(this Expression e, Dictionary<string, Expression> substitutions) => e.Accept(new VariableSubstitutor(substitutions));
        public static EquationOperation IsolateVariable(this Expression e, string variable) => e.Accept(new VariableIsolator(variable));
    }

    public interface IExpressionVisitor<T>
    {
        public T Visit(OpRational number);
        public T Visit(OpVariable variable);

        public T Visit(OpAdditiveInverse op);
        public T Visit(OpMultiplicativeInverse op);
        public T Visit(OpSum op);
        public T Visit(OpProduct op);
        public T Visit(OpLogarhithm op);
        public T Visit(OpPower op);
        public T Visit(OpRoot op);

        public T Visit(OpSin op);
        public T Visit(OpCos op);
        public T Visit(OpTg op);
        public T Visit(OpCotg op);
        public T Visit(OpSec op);
        public T Visit(OpCsc op);

        public T Visit(OpArcSin op);
        public T Visit(OpArcCos op);
        public T Visit(OpArcTg op);
        public T Visit(OpArcCotg op);
        public T Visit(OpArcSec op);
        public T Visit(OpArcCsc op);

        public T Visit(OpPi op);
        public T Visit(OpE op);
        public T Visit(OpParam op);
    }

    public class ExpressionCopier : IExpressionVisitor<Expression>
    {
        public Expression Visit(OpRational number)
        {
            return new OpRational(number.Numerator, number.Denominator);
        }
        public Expression Visit(OpVariable variable)
        {
            return new OpVariable(variable.Name);
        }

        public Expression Visit(OpAdditiveInverse op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpAdditiveInverse(e1);
        }
        public Expression Visit(OpMultiplicativeInverse op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpMultiplicativeInverse(e1);
        }
        public Expression Visit(OpSum op)
        {
            List<Expression> exps = new List<Expression>();
            foreach (Expression exp in op.Operands) exps.Add(exp.Accept(this));
            return new OpSum(exps.ToArray());
        }
        public Expression Visit(OpProduct op)
        {
            List<Expression> exps = new List<Expression>();
            foreach (Expression exp in op.Operands) exps.Add(exp.Accept(this));
            return new OpProduct(exps.ToArray());
        }
        public Expression Visit(OpLogarhithm op)
        {
            Expression e1 = op.OperandBase.Accept(this);
            Expression e2 = op.OperandArgument.Accept(this);
            return new OpLogarhithm(e1, e2);
        }
        public Expression Visit(OpPower op)
        {
            Expression e1 = op.OperandBase.Accept(this);
            Expression e2 = op.OperandExponent.Accept(this);
            return new OpPower(e1, e2);
        }
        public Expression Visit(OpRoot op)
        {
            Expression e1 = op.OperandExponent.Accept(this);
            Expression e2 = op.OperandBase.Accept(this);
            return new OpRoot(e1, e2);
        }

        public Expression Visit(OpSin op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpSin(e1);
        }
        public Expression Visit(OpCos op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpCos(e1);
        }
        public Expression Visit(OpTg op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpTg(e1);
        }
        public Expression Visit(OpCotg op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpCotg(e1);
        }
        public Expression Visit(OpSec op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpSec(e1);
        }
        public Expression Visit(OpCsc op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpCsc(e1);
        }

        public Expression Visit(OpArcSin op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpArcSin(e1);
        }
        public Expression Visit(OpArcCos op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpArcCos(e1);
        }
        public Expression Visit(OpArcTg op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpArcTg(e1);
        }
        public Expression Visit(OpArcCotg op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpArcCotg(e1);
        }
        public Expression Visit(OpArcSec op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpArcSec(e1);
        }
        public Expression Visit(OpArcCsc op)
        {
            Expression e1 = op.Operand.Accept(this);
            return new OpArcCsc(e1);
        }

        public Expression Visit(OpPi op)
        {
            return new OpPi();
        }
        public Expression Visit(OpE op)
        {
            return new OpE();
        }
        public Expression Visit(OpParam op)
        {
            return new OpParam(op.Name);
        }
    }
    public class ExpressionSimplifier : IExpressionVisitor<Tuple<bool, Expression>>
    {
        int depth = 0;

        public Tuple<bool, Expression> Visit(OpRational number)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Tries to simplify a rational number. Return true if any simplification occured.
            OpRational newRat = number.Simplify();

            depth--;
            if (newRat.Numerator == number.Numerator && newRat.Denominator == newRat.Denominator) return new Tuple<bool, Expression>(false, number);
            else return new Tuple<bool, Expression>(true, newRat);
        }
        public Tuple<bool, Expression> Visit(OpVariable variable)
        {
            // A varable by itself can't be simplified.
            return new Tuple<bool, Expression>(false, variable);
        }
        public Tuple<bool, Expression> Visit(OpAdditiveInverse op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Inverts a number, or multiplies by -1.

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);

            // Apply additive inverse to a number
            if (simpleOperand.Item2 is OpRational rat)
            {
                depth--;
                return new Tuple<bool, Expression>(true, rat.AditiveInverse);
            }

            // Othervise, rewrite as a multiple of -1
            else
            {
                depth--;
                OpProduct tg = new OpProduct(simpleOperand.Item2, new OpRational(-1, 1));
                return new Tuple<bool, Expression>(true, tg.Accept(this).Item2);
            }
        }
        public Tuple<bool, Expression> Visit(OpMultiplicativeInverse op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Inverts a number (failing when input is 0), or exponentiates by -1.

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);

            // Apply multiplicative inverse to a number
            if (simpleOperand.Item2 is OpRational rat)
            {
                depth--;
                return new Tuple<bool, Expression>(true, rat.MultiplicativeInverse);
            }

            // Othervise, rewrite as -1st power.
            else
            {
                depth--;
                OpPower pow = new OpPower(simpleOperand.Item2 , new OpRational(-1, 1));
                return new Tuple<bool, Expression>(true, pow.Accept(this).Item2);
            }
        }

        public Tuple<bool, Expression> Visit(OpSum op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            List<Expression> simpleOperands = new List<Expression>();
            bool anySimplified = false;

            foreach (Expression exp in op.Operands)
            {
                Tuple<bool, Expression> simp = exp.Accept(this);
                simpleOperands.Add(simp.Item2);
                anySimplified |= simp.Item1;
            }

            List<OpRational> rationals = new List<OpRational>();
            List<Tuple<Expression, Expression>> logs = new List<Tuple<Expression, Expression>>();
            List<Tuple<OpRational,Expression>> nonrationals = new List<Tuple<OpRational, Expression>>();

            // Add each term to the list of rationals, logarithms, or other expressions ('non-rationals')
            foreach (Expression exp in simpleOperands)
            {
                if (exp is OpSum tg)
                {
                    foreach (Expression exp0 in tg.Operands)
                    {
                        if (exp0 is OpRational rat0) rationals.Add(rat0);
                        else if (exp is OpLogarhithm log0) anySimplified |= Simplification.AddLogarhithm(logs, log0);
                        else Simplification.AddNonrationalTerm(nonrationals, exp0);
                    }
                }
                else if (exp is OpRational rat) rationals.Add(rat);
                else if (exp is OpLogarhithm log) anySimplified |= Simplification.AddLogarhithm(logs, log);
                else Simplification.AddNonrationalTerm(nonrationals, exp);
            }

            // Simplify logs and place them with other rationals or non-rationals based on result
            while(logs.Count > 0)
            {
                OpLogarhithm log = new OpLogarhithm(logs[0].Item1, logs[0].Item2);
                Tuple<bool, Expression> simpleLog = log.Accept(this);
                logs.RemoveAt(0);
                anySimplified |= simpleLog.Item1;
                if (simpleLog.Item2 is OpRational rat) rationals.Add(rat);
                else nonrationals.Add(new Tuple<OpRational, Expression>(new OpRational(1, 1), simpleLog.Item2));
            }

            if (rationals.Count > 1) anySimplified = true;

            OpRational sum = new OpRational(0, 1);

            foreach (OpRational rat in rationals) sum += rat;

            List<Expression> newTermList = new List<Expression>();
            foreach (Tuple<OpRational, Expression> t in nonrationals)
            {
                if (t.Item1.IsEquivalent(new OpRational(1, 1))) newTermList.Add(t.Item2);
                else if (t.Item2 is OpProduct)
                {
                    OpProduct clone = t.Item2.Clone() as OpProduct;
                    clone.Operands.Add(t.Item1);
                    newTermList.Add(clone);
                }
                else newTermList.Add(new OpProduct(t.Item1, t.Item2));
            }

            if (sum.Numerator != 0) newTermList.Add(sum);

            depth--;
            if (newTermList.Count == 0) return new Tuple<bool, Expression>(true, new OpRational(0, 1));
            else if (newTermList.Count == 1) return new Tuple<bool, Expression>(true, newTermList[0]);
            else return new Tuple<bool, Expression>(anySimplified, new OpSum(newTermList.ToArray()));
        }
        public Tuple<bool, Expression> Visit(OpProduct op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            if (op.Operands.Count == 0) return new Tuple<bool, Expression>(true, new OpRational(1, 1));

            List<Expression> simpleOperands = new List<Expression>();
            bool anySimplified = false;

            foreach (Expression exp in op.Operands)
            {
                Tuple<bool, Expression> simp = exp.Accept(this);
                simpleOperands.Add(simp.Item2);
                anySimplified |= simp.Item1;
            }

            List<OpRational> rationals = new List<OpRational>();
            List<Tuple<Expression, Expression>> powers = new List<Tuple<Expression, Expression>>();
            List<Tuple<Expression, Expression>> nonrationals = new List<Tuple<Expression, Expression>>();
            OpSum expand = null;

            foreach (Expression exp in simpleOperands)
            {
                if (expand == null && exp is OpSum a) expand = a;
                else if (exp is OpProduct tg)
                {
                    foreach (Expression exp0 in tg.Operands)
                    {
                        if (exp0 is OpRational rat0) rationals.Add(rat0);
                        else if (exp0 is OpPower pow0) anySimplified |= Simplification.AddPower(powers, pow0);
                        else if (exp0 is OpSum add0) Simplification.AddNonrationalFactor(nonrationals, add0);
                        else Simplification.AddFirstPower(powers, exp0);
                    }
                }
                else if (exp is OpRational rat) rationals.Add(rat);
                else if (exp is OpPower pow) anySimplified |= Simplification.AddPower(powers, pow);
                else if (exp is OpSum add) Simplification.AddNonrationalFactor(nonrationals, add);
                else Simplification.AddFirstPower(powers, exp);
            }

            // Contribute multiples from rationals to powers (eg. 9*3^(x+1) = 3^(x+3) ). Do it only for rational power-bases
            foreach (Tuple<Expression, Expression> pow in powers)
            {
                if (pow.Item1 is OpRational && pow.Item2.ListVariables().Count > 0)
                {
                    for (int i = 0; i < rationals.Count; i++)
                    {
                        (int, OpRational) res = Decomposition.GetPowerAndRemainder(pow.Item1 as OpRational, rationals[i]);
                        if (res.Item1 != 0)
                        {
                            rationals.RemoveAt(i);
                            rationals.Insert(i, res.Item2);

                            Simplification.AddPower(powers, new OpPower(pow.Item1.Clone(), new OpRational(res.Item1, 1)));
                        }
                    }
                }
            }

            // Simplify powers and place them with other rationals or non-rationals based on result
            while (powers.Count > 0)
            {
                OpPower pow = new OpPower(powers[0].Item1, powers[0].Item2);
                Tuple<bool, Expression> simplePow = pow.Accept(this);
                powers.RemoveAt(0);
                anySimplified |= simplePow.Item1;
                if (simplePow.Item2 is OpRational rat) rationals.Add(rat);
                else nonrationals.Add(new Tuple<Expression, Expression>(new OpRational(1, 1),simplePow.Item2));
            }

            if (rationals.Count > 1) anySimplified = true;

            OpRational product = new OpRational(1, 1);

            List<Expression> newFactorList = new List<Expression>();
            foreach (OpRational rat in rationals)
            {
                if (Complexity.ShouldMultiplicationBeCalculated(rat.Numerator, product.Numerator) &&
                    Complexity.ShouldMultiplicationBeCalculated(rat.Denominator, product.Denominator))
                {
                    product *= rat;
                }
                else Simplification.AddNonrationalFactor(nonrationals, rat);
            }

            if (product.Numerator != product.Denominator && product.Numerator != 0) newFactorList.Add(product);
            else if (product.Numerator == 0) return new Tuple<bool, Expression>(true, new OpRational(0, 1));

            foreach(Tuple<Expression, Expression> e in nonrationals)
            {
                if (e.Item1.IsEquivalent(new OpRational(1, 1))) newFactorList.Add(e.Item2);
                else newFactorList.Add(new OpPower(e.Item2, e.Item1));
            }

            if (expand == null)
            {
                depth--;
                if (newFactorList.Count == 0) return new Tuple<bool, Expression>(true, new OpRational(1, 1));
                else if (newFactorList.Count == 1) return new Tuple<bool, Expression>(true, newFactorList[0]);
                return new Tuple<bool, Expression>(anySimplified, new OpProduct(newFactorList.ToArray()));
            }
            else
            {
                if (newFactorList.Count == 0) return new Tuple<bool, Expression>(anySimplified, expand.SimplifyingCopy());

                else
                {
                    Expression expanded = newFactorList[0];
                    newFactorList.RemoveAt(0);
                    List<Expression> summands = new List<Expression>();

                    if (expanded is OpSum ag)
                    {
                        if (ag.Operands.Count * expand.Operands.Count <= Settings.MaxResultingTermsFromExpansion)
                        {
                            foreach (Expression exp in ag.Operands)
                            {
                                foreach (Expression exp2 in expand.Operands)
                                {
                                    Expression summand = new OpProduct(exp2, exp);
                                    Tuple<bool, Expression> simp = summand.Accept(this);
                                    anySimplified |= simp.Item1;
                                    summands.Add(simp.Item2);
                                }
                            }
                        }
                        else
                        {
                            summands.Add(new OpProduct(expanded, ag));
                        }
                    }
                    else
                    {
                        foreach (Expression exp2 in expand.Operands)
                        {
                            Expression summand = new OpProduct(exp2, expanded);
                            Tuple<bool, Expression> simp = summand.Accept(this);
                            anySimplified |= simp.Item1;
                            summands.Add(simp.Item2);
                        }
                    }

                    if (summands.Count > 1) newFactorList.Add(new OpSum(summands.ToArray()));
                    else if (summands.Count == 1) newFactorList.Add(summands[0]);
                    else newFactorList.Clear();

                }

                depth--;

                if (newFactorList.Count == 0) return new Tuple<bool, Expression>(true, new OpRational(0, 1));
                else if (newFactorList.Count == 1) return new Tuple<bool, Expression>(anySimplified, newFactorList[0]);
                return new Tuple<bool, Expression>(anySimplified, new OpProduct(newFactorList.ToArray()));
            }

            throw new SolverException("This shouldn't happen.");
        }
        public Tuple<bool, Expression> Visit(OpLogarhithm op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            Tuple<bool, Expression> baseExp = op.OperandBase.Accept(this);
            Tuple<bool, Expression> argument = op.OperandArgument.Accept(this);
            depth--;

            bool simplified = baseExp.Item1 | baseExp.Item1;

            // Log of 1 is equal to 0
            if (argument.Item2.IsEquivalent(new OpRational(1, 1)))
            {
                return new Tuple<bool, Expression>(true, new OpRational(0, 1));
            }

            // Log of its base is equal to 1
            if (argument.Item2.IsEquivalent(baseExp.Item2))
            {
                return new Tuple<bool, Expression>(true, new OpRational(1, 1));
            }

            // If a rational solution is know, rewrite log to the number (eg. log2(32) => 5)
            if (argument.Item2 is OpRational rat && baseExp.Item2 is OpRational rat1)
            {
                (int, OpRational) sol = Decomposition.GetPowerAndRemainder(rat1, rat);
                if (sol.Item2.IsEquivalent(new OpRational(1, 1))) return new Tuple<bool, Expression>(true, new OpRational(sol.Item1, 1));
            }

            // Log of a power of its base is the power's exponent (eg. ln(e^3) => 3)
            if (argument.Item2 is OpPower pow)
            {
                return new Tuple<bool, Expression>(true, pow.OperandExponent);
            }

            // No more rules are known
            return new Tuple<bool, Expression>(simplified, new OpLogarhithm(baseExp.Item2, argument.Item2));
        }
        public Tuple<bool, Expression> Visit(OpPower op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            Tuple<bool, Expression> b = op.OperandBase.Accept(this);
            Tuple<bool, Expression> exponent = op.OperandExponent.Accept(this);
            depth--;

            bool anySimplified = b.Item1 || exponent.Item1;
            // Calculate power of 1
            if (b.Item2 is OpRational rat5 && rat5.Numerator == 1 && rat5.Denominator == 1)
            {
                return new Tuple<bool, Expression>(true, new OpRational(1, 1));
            }
            // Calculate 1st power
            else if (exponent.Item2 is OpRational rat3 && rat3.Numerator == 1 && rat3.Denominator == 1)
            {
                return new Tuple<bool, Expression>(true, b.Item2);
            }
            // Calculate rational power
            else if (b.Item2 is OpRational rat0 && exponent.Item2 is OpRational rat1)
            {
                // Deal with negative sign in exponent
                if (rat1.Numerator < 0)
                {
                    try
                    {
                        rat0 = rat0.MultiplicativeInverse;
                        rat1 = rat1.AditiveInverse;
                    }
                    catch (DivideByZeroException)
                    {
                        throw new ArgumentException("Can't take negative power of zero");
                    }
                }

                // Deal with denominator 0
                if (rat0.Denominator == 0) throw new ArgumentException("Zero can't be denominator of a fraction.");

                // Reasonably large to evaluate?
                bool canBeEvaluated = Complexity.ShouldPowerBeCalculated(rat0.Numerator, rat1.Numerator) && Complexity.ShouldPowerBeCalculated(rat0.Denominator, rat1.Numerator);
                if (!canBeEvaluated) return new Tuple<bool, Expression>(false, op);

                int power = (int)rat1.Numerator;
                int root = (int)rat1.Denominator;
                BigInteger numerator1 = rat0.Numerator;
                BigInteger denominator1 = rat0.Denominator;

                if (root == 1)
                {
                    return new Tuple<bool, Expression>(true, new OpRational(BigInteger.Pow(numerator1, power), BigInteger.Pow(denominator1, power)));
                }

                if (numerator1 == 0 && denominator1 != 0) return new Tuple<bool, Expression>(true, new OpRational(0, 1));

                Expression numeratorR = Decomposition.PartialRootFull(root, power, numerator1);
                if (numeratorR is OpProduct otg && otg.Operands.Count == 1) numeratorR = otg.Operands[0];
                else if (numeratorR is OpProduct otg1 && otg1.Operands.Count == 0) numeratorR = new OpRational(1, 1);
                Expression denominatorR = Decomposition.PartialRootFull(root, power, denominator1);
                if (denominatorR is OpProduct otg2 && otg2.Operands.Count == 1) denominatorR = otg2.Operands[0];
                else if (denominatorR is OpProduct otg3 && otg3.Operands.Count == 0) denominatorR = new OpRational(1,1);

                Expression toReturn;

                if (!denominatorR.IsEquivalent(new OpRational(-1, 1)))
                {
                    toReturn = numeratorR;
                }
                else toReturn = new OpProduct(numeratorR, denominatorR);

                if (anySimplified)
                {
                    return new Tuple<bool, Expression>(true, toReturn);
                }
                else return new Tuple<bool, Expression>(false, toReturn);
            }
            // Calculate power of a log
            else if (exponent.Item2 is OpLogarhithm log && log.OperandBase.IsEquivalent(b.Item2))
            {
                return new Tuple<bool, Expression>(true, log.OperandArgument);
            }
            // Calculate power of a power
            else if (b.Item2 is OpPower pow2)
            {
                return new Tuple<bool, Expression>(true, new OpPower(pow2.OperandBase, new OpProduct(pow2.OperandExponent, exponent.Item2).Accept(this).Item2));
            }
            // Calculate 0th power
            else if (exponent.Item2 is OpRational rat4 && rat4.Numerator == 0 && rat4.Denominator == 1)
            {
                return new Tuple<bool, Expression>(true, new OpRational(1, 1));
            }
            // Distribute power over a product
            else if (exponent.Item2 is OpRational rat2 && b.Item2 is OpProduct group)
            {
                OpProduct toReturn = new OpProduct();

                foreach (Expression exp in group.Operands)
                {
                    toReturn.Operands.Add(new OpPower(exp, rat2.Clone()).Accept(this).Item2);
                }

                Expression toReturn2 = toReturn.Accept(this).Item2;
                return new Tuple<bool, Expression>(true, toReturn2);
            }
            // Expand power of sum (eg. (a+b)^3=a^3+3a^2+... )
            else if (b.Item2 is OpSum && exponent.Item2 is OpRational rat6 && rat6.Denominator == 1 && rat6.Numerator >= 2
                && Settings.MaxResultingTermsFromExpansion >= Math.Pow((b.Item2 as OpSum).Operands.Count, (int)rat6.Numerator))
            {
                OpProduct expanded = new OpProduct();
                for (int i = 0; i < (int)rat6.Numerator; i++)
                {
                    expanded.Operands.Add(b.Item2.Clone());
                }
                expanded.Accept(this);
                return new Tuple<bool, Expression>(true, expanded);

            }
            // No more simplifications are known
            else return new Tuple<bool, Expression>(anySimplified, new OpPower(b.Item2, exponent.Item2));
        }
        public Tuple<bool, Expression> Visit(OpRoot op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Checks if simplifing given root makes sense.
            // If so, simplifies it as a power.

            Tuple<bool, Expression> radicant = op.OperandBase.Accept(this);
            Tuple<bool, Expression> exponent = op.OperandExponent.Accept(this);

            if (exponent.Item2.IsEquivalent(new OpRational(0,1)))
            {
                throw new SolverException("Can't take 0th root.");
            }

            // Calculates the root by calculating a power.
            OpPower power = new OpPower(radicant.Item2, new OpMultiplicativeInverse(exponent.Item2));

            depth--;
            return power.Accept(this);
        }

        public Tuple<bool, Expression> Visit(OpSin op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);

            // Use table values (eg. sin(pi/3) = sqrt(3)/2)
            OpRational rat = Simplification.IsRationalMultipleOfPi(simpleOperand.Item2);
            if (rat != null)
            {
                Expression result = Simplification.LookUpSin(rat);
                if (result != null)
                {
                    depth--;
                    return new Tuple<bool, Expression>(true, result);
                }
            }

            // Use sum formula sin(a+b) = sin(a)cos(b) + cos(b)sin(a)
            else if (simpleOperand.Item2 is OpSum sum)
            {
                if (sum.Operands.Count == 2)
                {
                    depth--;
                    return new Tuple<bool, Expression>(true,
                        new OpSum(
                            new OpProduct(new OpSin(sum.Operands[0].Clone()), new OpCos(sum.Operands[1].Clone())),
                            new OpProduct(new OpSin(sum.Operands[1].Clone()), new OpCos(sum.Operands[0].Clone()))
                            ));
                }
                else if (sum.Operands.Count <= Settings.MaxTermsForTrigonometricSumExpansion)
                {
                    OpSum moreTerms = new OpSum();
                    for (int i = 1; i < sum.Operands.Count; i++)
                    {
                        moreTerms.Operands.Add(sum.Operands[i]);
                    }

                    depth--;
                    return new Tuple<bool, Expression>(true,
                        new OpSum(
                            new OpProduct(new OpSin(sum.Operands[0].Clone()), new OpCos(moreTerms.Clone())),
                            new OpProduct(new OpSin(moreTerms.Clone()), new OpCos(sum.Operands[0].Clone()))
                            ));
                }
            }

            // Can't reduce anymore
            depth--;
            return new Tuple<bool, Expression>(simpleOperand.Item1, new OpSin(simpleOperand.Item2));
        }
        public Tuple<bool, Expression> Visit(OpCos op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);

            // Use table values (eg. cos(pi/3) = 1/2)
            OpRational rat = Simplification.IsRationalMultipleOfPi(simpleOperand.Item2);
            if (rat != null)
            {
                Expression result = Simplification.LookUpCos(rat);
                if (result != null)
                {
                    depth--;
                    return new Tuple<bool, Expression>(true, result);
                }
            }

            // Use sum formula cos(a+b) = cos(a)cos(b) + sin(b)sin(a)
            else if (simpleOperand.Item2 is OpSum sum)
            {
                if (sum.Operands.Count == 2)
                {
                    depth--;
                    return new Tuple<bool, Expression>(true,
                        new OpSum(
                            new OpProduct(new OpCos(sum.Operands[0].Clone()), new OpCos(sum.Operands[1].Clone())),
                            new OpProduct(new OpSin(sum.Operands[1].Clone()), new OpSin(sum.Operands[0].Clone()))
                            ));
                }
                else if (sum.Operands.Count <= Settings.MaxTermsForTrigonometricSumExpansion)
                {
                    OpSum moreTerms = new OpSum();
                    for (int i = 1; i < sum.Operands.Count; i++)
                    {
                        moreTerms.Operands.Add(sum.Operands[i]);
                    }

                    depth--;
                    return new Tuple<bool, Expression>(true,
                        new OpSum(
                            new OpProduct(new OpCos(sum.Operands[0].Clone()), new OpCos(moreTerms.Clone())),
                            new OpProduct(new OpSin(moreTerms.Clone()), new OpSin(sum.Operands[0].Clone()))
                            ));
                }
            }

            // Can't reduce anymore
            depth--;
            return new Tuple<bool, Expression>(simpleOperand.Item1, new OpCos(simpleOperand.Item2));
        }
        public Tuple<bool, Expression> Visit(OpTg op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as sin(x)/cos(x), then simplify that

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpProduct
                (
                new OpSin(simpleOperand.Item2.Clone()),
                new OpMultiplicativeInverse(new OpCos(simpleOperand.Item2.Clone()))
                );

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }
        public Tuple<bool, Expression> Visit(OpCotg op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as cos(x)/sin(x), then simplify that

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpProduct
                (
                new OpCos(simpleOperand.Item2.Clone()),
                new OpMultiplicativeInverse(new OpSin(simpleOperand.Item2.Clone()))
                );

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }
        public Tuple<bool, Expression> Visit(OpSec op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as 1/cos(x), then simplify that

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpMultiplicativeInverse(new OpCos(simpleOperand.Item2.Clone()));

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }
        public Tuple<bool, Expression> Visit(OpCsc op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as 1/cos(x), then simplify that

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpMultiplicativeInverse(new OpSin(simpleOperand.Item2.Clone()));

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }

        public Tuple<bool, Expression> Visit(OpArcSin op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // If lookup table can simplify the expression, then simplify it.
            // Othervise retrun the expression

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            List<Expression> results = Simplification.LookUpArcSin(simpleOperand.Item2);

            depth--;
            if (results.Count == 0) return new Tuple<bool, Expression>(simpleOperand.Item1, new OpArcSin(simpleOperand.Item2));
            else return new Tuple<bool, Expression>(true, results[0]);
        }
        public Tuple<bool, Expression> Visit(OpArcCos op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // If lookup table can simplify the expression, then simplify it.
            // Othervise retrun the expression

            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            List<Expression> results = Simplification.LookUpArcCos(simpleOperand.Item2);

            depth--;
            if (results.Count == 0) return new Tuple<bool, Expression>(simpleOperand.Item1, new OpArcCos(simpleOperand.Item2));
            else return new Tuple<bool, Expression>(true, results[0]);
        }
        public Tuple<bool, Expression> Visit(OpArcTg op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as arcsin(x/sqrt(1+x^2)), then simplify that
            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpArcSin(new OpProduct(simpleOperand.Item2.Clone(), new OpMultiplicativeInverse(new OpPower(new OpSum(new OpRational(1,1), new OpPower(simpleOperand.Item2.Clone(), new OpRational(2,1))), new OpRational(1, 2)))));

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }
        public Tuple<bool, Expression> Visit(OpArcCotg op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as arccos(x/sqrt(1+x^2)), then simplify that
            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpArcCos(new OpProduct(simpleOperand.Item2.Clone(), new OpMultiplicativeInverse(new OpPower(new OpSum(new OpRational(1, 1), new OpPower(simpleOperand.Item2.Clone(), new OpRational(2, 1))), new OpRational(1, 2)))));

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }
        public Tuple<bool, Expression> Visit(OpArcSec op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as arccos(1/x), then simplify that
            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpArcCos(new OpMultiplicativeInverse(simpleOperand.Item2));

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }
        public Tuple<bool, Expression> Visit(OpArcCsc op)
        {
            // If we got too deep, we might be going in circle
            depth++;
            if (depth >= Settings.MaxDepthForSimplification) throw new SolverException("Simplification resulted in supposedly infinite loop.");


            // Rewrite as arcsin(1/x), then simplify that
            Tuple<bool, Expression> simpleOperand = op.Operand.Accept(this);
            Expression ret = new OpArcSin(new OpMultiplicativeInverse(simpleOperand.Item2));

            Tuple<bool, Expression> ret2 = ret.Accept(this);

            depth--;
            return new Tuple<bool, Expression>(true, ret2.Item2);
        }

        public Tuple<bool, Expression> Visit(OpPi op) => new Tuple<bool, Expression>(false, op);
        public Tuple<bool, Expression> Visit(OpE op) => new Tuple<bool, Expression>(false, op);
        public Tuple<bool, Expression> Visit(OpParam op) => new Tuple<bool, Expression>(false, op);
    }
    public class RealNumberExpressionChecker : IExpressionVisitor<bool>
    {
        public bool Visit(OpRational number) => true;
        public bool Visit(OpVariable variable) => false;

        public bool Visit(OpAdditiveInverse op) => op.Operand.Accept(this);
        public bool Visit(OpMultiplicativeInverse op) => op.Operand.Accept(this);
        public bool Visit(OpSum op)
        {
            bool b = true;
            foreach (Expression exp in op.Operands) b &= exp.Accept(this);
            return b;
        }
        public bool Visit(OpProduct op)
        {
            bool b = true;
            foreach (Expression exp in op.Operands) b &= exp.Accept(this);
            return b;
        }
        public bool Visit(OpLogarhithm op) => op.OperandBase.Accept(this) && op.OperandArgument.Accept(this);
        public bool Visit(OpPower op) => op.OperandBase.Accept(this) && op.OperandExponent.Accept(this);
        public bool Visit(OpRoot op) => op.OperandExponent.Accept(this) && op.OperandBase.Accept(this);

        public bool Visit(OpSin op) => op.Operand.Accept(this);
        public bool Visit(OpCos op) => op.Operand.Accept(this);
        public bool Visit(OpTg op) => op.Operand.Accept(this);
        public bool Visit(OpCotg op) => op.Operand.Accept(this);
        public bool Visit(OpSec op) => op.Operand.Accept(this);
        public bool Visit(OpCsc op) => op.Operand.Accept(this);

        public bool Visit(OpArcSin op) => op.Operand.Accept(this);
        public bool Visit(OpArcCos op) => op.Operand.Accept(this);
        public bool Visit(OpArcTg op) => op.Operand.Accept(this);
        public bool Visit(OpArcCotg op) => op.Operand.Accept(this);
        public bool Visit(OpArcSec op) => op.Operand.Accept(this);
        public bool Visit(OpArcCsc op) => op.Operand.Accept(this);

        public bool Visit(OpPi op) => true;
        public bool Visit(OpE op) => true;
        public bool Visit(OpParam op) => true;
    }
    public class VariableLister : IExpressionVisitor<HashSet<string>>
    {
        public HashSet<string> Visit(OpRational number) => new HashSet<string>();
        public HashSet<string> Visit(OpVariable variable) => new HashSet<string>() { variable.Name };

        public HashSet<string> Visit(OpAdditiveInverse op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpMultiplicativeInverse op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpSum op)
        {
            HashSet<string> hs1 = new HashSet<string>();
            foreach(Expression exp in op.Operands) hs1.UnionWith(exp.Accept(this));
            return hs1;
        }
        public HashSet<string> Visit(OpProduct op)
        {
            HashSet<string> hs1 = new HashSet<string>();
            foreach (Expression exp in op.Operands) hs1.UnionWith(exp.Accept(this));
            return hs1;
        }
        public HashSet<string> Visit(OpLogarhithm op)
        {
            HashSet<string> hs1 = op.OperandBase.Accept(this);
            HashSet<string> hs2 = op.OperandArgument.Accept(this);
            hs1.UnionWith(hs2);
            return hs1;
        }
        public HashSet<string> Visit(OpPower op)
        {
            HashSet<string> hs1 = op.OperandBase.Accept(this);
            HashSet<string> hs2 = op.OperandExponent.Accept(this);
            hs1.UnionWith(hs2);
            return hs1;
        }
        public HashSet<string> Visit(OpRoot op)
        {
            HashSet<string> hs1 = op.OperandExponent.Accept(this);
            HashSet<string> hs2 = op.OperandBase.Accept(this);
            hs1.UnionWith(hs2);
            return hs1;
        }

        public HashSet<string> Visit(OpSin op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpCos op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpTg op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpCotg op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpSec op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpCsc op) => op.Operand.Accept(this);

        public HashSet<string> Visit(OpArcSin op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpArcCos op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpArcTg op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpArcCotg op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpArcSec op) => op.Operand.Accept(this);
        public HashSet<string> Visit(OpArcCsc op) => op.Operand.Accept(this);

        public HashSet<string> Visit(OpPi op) => new HashSet<string>();
        public HashSet<string> Visit(OpE op) => new HashSet<string>();
        public HashSet<string> Visit(OpParam op) => new HashSet<string>();
    }
    public class VariableSubstitutor : IExpressionVisitor<Expression>
    {
        readonly Dictionary<string, Expression> substitutions;

        public VariableSubstitutor(Dictionary<string, Expression> substitutions)
        {
            this.substitutions = substitutions;
        }

        public Expression Visit(OpRational number) => number;
        public Expression Visit(OpVariable variable)
        {
            if (substitutions.ContainsKey(variable.Name)) return substitutions[variable.Name];
            else return variable;
        }

        public Expression Visit(OpAdditiveInverse op) => new OpAdditiveInverse(op.Operand.Accept(this));
        public Expression Visit(OpMultiplicativeInverse op) => new OpMultiplicativeInverse(op.Operand.Accept(this));
        public Expression Visit(OpSum op)
        {
            List<Expression> exps = new List<Expression>();
            foreach (Expression exp in op.Operands) exps.Add(exp.Accept(this));
            return new OpSum(exps.ToArray());
        }
        public Expression Visit(OpProduct op)
        {
            List<Expression> exps = new List<Expression>();
            foreach (Expression exp in op.Operands) exps.Add(exp.Accept(this));
            return new OpProduct(exps.ToArray());
        }

        public Expression Visit(OpLogarhithm op)
        {
            Expression ex1 = op.OperandBase.Accept(this);
            Expression ex2 = op.OperandArgument.Accept(this);
            return new OpLogarhithm(ex1, ex2);
        }
        public Expression Visit(OpPower op)
        {
            Expression ex1 = op.OperandBase.Accept(this);
            Expression ex2 = op.OperandExponent.Accept(this);
            return new OpPower(ex1, ex2);
        }
        public Expression Visit(OpRoot op)
        {
            Expression ex1 = op.OperandExponent.Accept(this);
            Expression ex2 = op.OperandBase.Accept(this);
            return new OpRoot(ex1, ex2);
        }

        public Expression Visit(OpSin op) => new OpSin(op.Operand.Accept(this));
        public Expression Visit(OpCos op) => new OpCos(op.Operand.Accept(this));
        public Expression Visit(OpTg op) => new OpTg(op.Operand.Accept(this));
        public Expression Visit(OpCotg op) => new OpCotg(op.Operand.Accept(this));
        public Expression Visit(OpSec op) => new OpSec(op.Operand.Accept(this));
        public Expression Visit(OpCsc op) => new OpCsc(op.Operand.Accept(this));

        public Expression Visit(OpArcSin op) => new OpArcSin(op.Operand.Accept(this));
        public Expression Visit(OpArcCos op) => new OpArcCos(op.Operand.Accept(this));
        public Expression Visit(OpArcTg op) => new OpArcTg(op.Operand.Accept(this));
        public Expression Visit(OpArcCotg op) => new OpArcCotg(op.Operand.Accept(this));
        public Expression Visit(OpArcSec op) => new OpArcSec(op.Operand.Accept(this));
        public Expression Visit(OpArcCsc op) => new OpArcCsc(op.Operand.Accept(this));

        public Expression Visit(OpPi op) => op;
        public Expression Visit(OpE op) => op;
        public Expression Visit(OpParam op) => op;
    }
    public class VariableIsolator : IExpressionVisitor<EquationOperation>
    {
        readonly string variable;

        public VariableIsolator(string variable)
        {
            this.variable = variable;
        }

        public EquationOperation Visit(OpRational number)
        {
            return new EquationOperation(EquationOperationType.none, null, null);
        }
        public EquationOperation Visit(OpVariable variable)
        {
            return new EquationOperation(EquationOperationType.none, null, null);
        }

        public EquationOperation Visit(OpAdditiveInverse op)
        {
            return new EquationOperation(EquationOperationType.flipSign, null, op.Operand);
        }
        public EquationOperation Visit(OpMultiplicativeInverse op)
        {
            return new EquationOperation(EquationOperationType.invert, null, op.Operand);
        }
        public EquationOperation Visit(OpSum op)
        {
            foreach (Expression exp in op.Operands)
            {
                if (!exp.ListVariables().Contains(variable))
                {
                    List<Expression> newOperands = new List<Expression>(op.Operands);
                    newOperands.Remove(exp);
                    return new EquationOperation(EquationOperationType.minus, exp, new OpSum(newOperands.ToArray()));
                }
            }
            return new EquationOperation(EquationOperationType.evalPolynomial, null, null);
        }
        public EquationOperation Visit(OpProduct op)
        {
            foreach (Expression exp in op.Operands)
            {
                if (!exp.ListVariables().Contains(variable))
                {
                    List<Expression> newOperands = new List<Expression>(op.Operands);
                    newOperands.Remove(exp);
                    return new EquationOperation(EquationOperationType.divide, exp, new OpProduct(newOperands.ToArray()));
                }
            }
            throw new SolverException("Couldn't isolate a variable in a product");
        }
        public EquationOperation Visit(OpLogarhithm op)
        {
            bool left = op.OperandBase.ListVariables().Contains(variable);
            bool right = op.OperandArgument.ListVariables().Contains(variable);

            if (left && !right)
            {
                return new EquationOperation(EquationOperationType.powerFromLeft, op.OperandBase.Clone(), op.OperandArgument.Clone());
            }
            else if (right && !left)
            {
                return new EquationOperation(EquationOperationType.powerFromLeft, op.OperandBase.Clone(), op.OperandArgument.Clone());
            }
            else if (right && left)
            {
                throw new SolverException("Can't solve equations where an unknown variable occurs in both the arguement and the base of a logarithm.");
            }
            return new EquationOperation(EquationOperationType.none, null, null);
        }
        public EquationOperation Visit(OpPower op)
        {
            bool left = op.OperandBase.ListVariables().Contains(variable);
            bool right = op.OperandExponent.ListVariables().Contains(variable);

            if (left && !right)
            {
                return new EquationOperation(EquationOperationType.root, op.OperandExponent.Clone(), op.OperandBase.Clone());
            }
            else if (right && !left)
            {
                return new EquationOperation(EquationOperationType.logarithm, op.OperandBase.Clone(), op.OperandExponent.Clone());
            }
            else if (right && left)
            {
                throw new SolverException("Can't solve equations where an unknown variable occurs both as the exponent and the base of a power.");
            }
            return new EquationOperation(EquationOperationType.none, null, null);
        }
        public EquationOperation Visit(OpRoot op)
        {
            bool left = op.OperandExponent.ListVariables().Contains(variable);
            bool right = op.OperandBase.ListVariables().Contains(variable);

            // Apply log
            if (left && !right)
            {
                return new EquationOperation(EquationOperationType.logarithm, op.OperandBase, new OpMultiplicativeInverse(op.OperandBase.Clone()));
            }
            // Exponentiate both sides
            else if (right && !left)
            {
                return new EquationOperation(EquationOperationType.power, op.OperandExponent, op.OperandBase.Clone());
            }
            else if (right && left)
            {
                throw new SolverException("Can't solve equations where an unknown variable occurs both as the exponent and the base of a root.");
            }
            return new EquationOperation(EquationOperationType.none, null, null);
        }

        public EquationOperation Visit(OpSin op) => new EquationOperation(EquationOperationType.evalTrigTable, null, null);
        public EquationOperation Visit(OpCos op) => new EquationOperation(EquationOperationType.evalTrigTable, null, null);
        public EquationOperation Visit(OpTg op) => new EquationOperation(EquationOperationType.arctg, null, op.Operand);
        public EquationOperation Visit(OpCotg op) => new EquationOperation(EquationOperationType.arccotg, null, op.Operand);
        public EquationOperation Visit(OpSec op) => new EquationOperation(EquationOperationType.arcsec, null, op.Operand);
        public EquationOperation Visit(OpCsc op) => new EquationOperation(EquationOperationType.arccsc, null, op.Operand);

        public EquationOperation Visit(OpArcSin op) => new EquationOperation(EquationOperationType.sin, null, op.Operand);
        public EquationOperation Visit(OpArcCos op) => new EquationOperation(EquationOperationType.cos, null, op.Operand);
        public EquationOperation Visit(OpArcTg op) => new EquationOperation(EquationOperationType.tg, null, op.Operand);
        public EquationOperation Visit(OpArcCotg op) => new EquationOperation(EquationOperationType.cotg, null, op.Operand);
        public EquationOperation Visit(OpArcSec op) => new EquationOperation(EquationOperationType.sec, null, op.Operand);
        public EquationOperation Visit(OpArcCsc op) => new EquationOperation(EquationOperationType.csc, null, op.Operand);

        public EquationOperation Visit(OpPi op) => new EquationOperation(EquationOperationType.none, null, null);
        public EquationOperation Visit(OpE op) => new EquationOperation(EquationOperationType.none, null, null);
        public EquationOperation Visit(OpParam op) => new EquationOperation(EquationOperationType.none, null, null);
    }
    public class DoubleEvaluator : IExpressionVisitor<double>
    {
        public double Visit(OpRational number) => (double)number.Numerator / (double)number.Denominator;
        public double Visit(OpVariable variable) => 0;

        public double Visit(OpAdditiveInverse op) => -op.Operand.Accept(this);
        public double Visit(OpMultiplicativeInverse op) => 1 / op.Operand.Accept(this);

        public double Visit(OpSum op)
        {
            double result = 0;
            foreach (Expression exp in op.Operands) result += exp.Accept(this);
            return result;
        }
        public double Visit(OpProduct op)
        {
            double result = 1;
            foreach (Expression exp in op.Operands) result *= exp.Accept(this);
            return result;
        }

        public double Visit(OpLogarhithm op)
        {
            double op1 = op.OperandBase.Accept(this);
            double op2 = op.OperandArgument.Accept(this);

            return Math.Log(op2, op1);
        }
        public double Visit(OpPower op)
        {
            double op1 = op.OperandBase.Accept(this);
            double op2 = op.OperandExponent.Accept(this);

            return Math.Pow(op1, op2);
        }
        public double Visit(OpRoot op)
        {
            double op1 = op.OperandBase.Accept(this);
            double op2 = op.OperandExponent.Accept(this);

            return Math.Pow(op1, 1/op2);
        }

        public double Visit(OpSin op) => Math.Sin(op.Operand.Accept(this));
        public double Visit(OpCos op) => Math.Cos(op.Operand.Accept(this));
        public double Visit(OpTg op) => Math.Tan(op.Operand.Accept(this));
        public double Visit(OpCotg op) => 1 / Math.Tan(op.Operand.Accept(this));
        public double Visit(OpSec op) => 1 / Math.Cos(op.Operand.Accept(this));
        public double Visit(OpCsc op) => 1 / Math.Sin(op.Operand.Accept(this));

        public double Visit(OpArcSin op) => Math.Asin(op.Operand.Accept(this));
        public double Visit(OpArcCos op) => Math.Acos(op.Operand.Accept(this));
        public double Visit(OpArcTg op) => Math.Atan(op.Operand.Accept(this));
        public double Visit(OpArcCotg op) => Math.Atan(1 / op.Operand.Accept(this));
        public double Visit(OpArcSec op) => Math.Acos(1 / op.Operand.Accept(this));
        public double Visit(OpArcCsc op) => Math.Asin(1 / op.Operand.Accept(this));

        public double Visit(OpPi op) => Math.PI;
        public double Visit(OpE op) => Math.E;
        public double Visit(OpParam op) => 0;
    }

    public class OpRational : Expression, IComparable
    {
        public BigInteger Numerator { get; private set; }
        public BigInteger Denominator { get; private set; }
        public OpRational(BigInteger numerator, BigInteger denominator)
        {
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpRational)
            {
                // Note: 1/2 and 2/4 aren't considered as equal.
                return Numerator == (v as OpRational).Numerator && Denominator == (v as OpRational).Denominator;
            }
            return false;
        }

        public OpRational Simplify()
        {
            // Use only factors up to considered factor
            (BigInteger, BigInteger) simple = Decomposition.CancelFraction(Numerator, Denominator);
            return new OpRational(simple.Item1, simple.Item2);
        }
        public OpRational AditiveInverse => new OpRational(-Numerator, Denominator);
        public OpRational MultiplicativeInverse
        {
            get
            {
                if (Numerator != 0) return new OpRational(Denominator, Numerator);
                else throw new DivideByZeroException("Nulou nelze dělit.");
            }
        }
        public static OpRational operator+(OpRational first, OpRational second)
        {
            OpRational rat = new OpRational(
                first.Numerator * second.Denominator +
                second.Numerator * first.Denominator,
                first.Denominator * second.Denominator);
            return rat.Simplify();
        }
        public static OpRational operator*(OpRational first, OpRational second)
        {
            OpRational rat = new OpRational(
                first.Numerator * second.Numerator,
                first.Denominator * second.Denominator);
            return rat.Simplify();
        }

        public override string ToString()
        {
            int sign = Numerator.Sign * Denominator.Sign;
            if (sign == 0) return "0";
            else
            {
                string s = "";

                if (Denominator == 1) s += Numerator.ToString();
                else s += (Numerator.ToString() + "/" + Denominator.ToString());

                return s;
            }
        }
        public int CompareTo(object obj)
        {
            if (obj is OpRational v)
            {
                return (Numerator * v.Denominator).CompareTo(Denominator * v.Numerator);
            }
            throw new ArgumentException("Can't compare rational number with other types.");
        }
    }
    public class OpVariable : Expression
    {
        public readonly string Name;
        public OpVariable(string name)
        {
            this.Name = name;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpVariable)
            {
                return Name == (v as OpVariable).Name;
            }
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class OpAdditiveInverse : Expression
    {
        public readonly Expression Operand;
        public OpAdditiveInverse(Expression operand)
        {
            this.Operand = operand;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpAdditiveInverse)
            {
                return Operand.IsEquivalent((v as OpAdditiveInverse).Operand);
            }
            return false;
        }

        public override string ToString()
        {
            return "-" + Operand.ToString();
        }
    }
    public class OpMultiplicativeInverse : Expression
    {
        public readonly Expression Operand;
        public OpMultiplicativeInverse(Expression operand)
        {
            this.Operand = operand;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpMultiplicativeInverse)
            {
                return Operand.IsEquivalent((v as OpMultiplicativeInverse).Operand);
            }
            return false;
        }

        public override string ToString()
        {
            return Operand.ToString() + "^(-1)"; 
        }
    }
    public class OpLogarhithm : Expression
    {
        public readonly Expression OperandBase;
        public readonly Expression OperandArgument;
        public OpLogarhithm(Expression @base, Expression argument)
        {
            this.OperandBase = @base;
            this.OperandArgument = argument;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpLogarhithm)
            {
                return OperandBase.IsEquivalent((v as OpLogarhithm).OperandBase) && OperandArgument.IsEquivalent((v as OpLogarhithm).OperandArgument);
            }
            return false;
        }

        public override string ToString()
        {
            return "log(" + OperandBase.ToString() + "," + OperandArgument.ToString() + ")";
        }
    }
    public class OpPower : Expression
    {
        public readonly Expression OperandBase;
        public readonly Expression OperandExponent;
        public OpPower(Expression @base, Expression exponent)
        {
            this.OperandBase = @base;
            this.OperandExponent = exponent;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpPower)
            {
                return OperandBase.IsEquivalent((v as OpPower).OperandBase) && OperandExponent.IsEquivalent((v as OpPower).OperandExponent);
            }
            return false;
        }

        public override string ToString()
        {
            return "(" + OperandBase.ToString() + "^" + OperandExponent.ToString() + ")";
        }
    }
    public class OpRoot : Expression
    {
        public readonly Expression OperandExponent;
        public readonly Expression OperandBase;
        public OpRoot(Expression exponent, Expression @base)
        {
            this.OperandExponent = exponent;
            this.OperandBase = @base;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpRoot)
            {
                return OperandExponent.IsEquivalent((v as OpRoot).OperandExponent) && OperandBase.IsEquivalent((v as OpRoot).OperandBase);
            }
            return false;
        }

        public override string ToString()
        {
            return "root(" + OperandExponent.ToString() + "," + OperandBase.ToString() + ")";
        }
    }

    public class OpProduct : Expression
    {
        public OpProduct(params Expression[] expressions)
        {
            Operands = new List<Expression>(expressions);
        }

        public readonly List<Expression> Operands;
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpProduct g)
            {
                if (Operands.Count == g.Operands.Count)
                {
                    for (int i = 0; i < Operands.Count; i++)
                    {
                        if (!Operands[i].IsEquivalent(g.Operands[i])) return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public override string ToString()
        {
            if (Operands.Count == 0) return "(1)";

            string s = Operands[0].ToString();
            for (int i = 1; i < Operands.Count; i++)
            {
                s += "*" + Operands[i];
            }
            return "(" + s + ")";
        }
    }
    public class OpSum : Expression
    {
        public OpSum(params Expression[] expressions)
        {
            Operands = new List<Expression>(expressions);
        }

        public readonly List<Expression> Operands;
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpSum g)
            {
                if (Operands.Count == g.Operands.Count)
                {
                    for (int i = 0; i < Operands.Count; i++)
                    {
                        if (!Operands[i].IsEquivalent(g.Operands[i])) return false;
                    }
                    return true;
                }
            }
            return false;
        }
        public override string ToString()
        {
            if (Operands.Count == 0) return "(0)";
            string s = Operands[0].ToString();
            for (int i = 1; i < Operands.Count; i++)
            {
                s += "+" + Operands[i];
            }
            return "(" + s + ")";
        }
    }

    public class OpSin : Expression
    {
        public readonly Expression Operand;

        public OpSin(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpSin g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "sin(" + Operand.ToString() + ")";
        }
    }
    public class OpCos : Expression
    {
        public readonly Expression Operand;

        public OpCos(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpCos g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "cos(" + Operand.ToString() + ")";
        }
    }
    public class OpTg : Expression
    {
        public readonly Expression Operand;

        public OpTg(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpTg g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "tg(" + Operand.ToString() + ")";
        }
    }
    public class OpCotg : Expression
    {
        public readonly Expression Operand;

        public OpCotg(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpCotg g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "cotg(" + Operand.ToString() + ")";
        }
    }
    public class OpSec : Expression
    {
        public readonly Expression Operand;

        public OpSec(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpSec g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "sec(" + Operand.ToString() + ")";
        }
    }
    public class OpCsc : Expression
    {
        public readonly Expression Operand;

        public OpCsc(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpCsc g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "csc(" + Operand.ToString() + ")";
        }
    }

    public class OpArcSin : Expression
    {
        public readonly Expression Operand;

        public OpArcSin(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpArcSin g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "arcsin(" + Operand.ToString() + ")";
        }
    }
    public class OpArcCos : Expression
    {
        public readonly Expression Operand;

        public OpArcCos(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpArcCos g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "arccos(" + Operand.ToString() + ")";
        }
    }
    public class OpArcTg : Expression
    {
        public readonly Expression Operand;

        public OpArcTg(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpArcTg g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "arctg(" + Operand.ToString() + ")";
        }
    }
    public class OpArcCotg : Expression
    {
        public readonly Expression Operand;

        public OpArcCotg(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpArcCotg g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "arccotg(" + Operand.ToString() + ")";
        }
    }
    public class OpArcSec : Expression
    {
        public readonly Expression Operand;

        public OpArcSec(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpArcSec g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "arcsec(" + Operand.ToString() + ")";
        }
    }
    public class OpArcCsc : Expression
    {
        public readonly Expression Operand;

        public OpArcCsc(Expression operand)
        {
            Operand = operand;
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpArcCsc g)
            {
                return Operand.IsEquivalent(g.Operand);
            }
            return false;
        }
        public override string ToString()
        {
            return "arccsc(" + Operand.ToString() + ")";
        }
    }

    public class OpPi : Expression
    {
        public OpPi()
        {
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpPi)
            {
                return true;
            }
            return false;
        }
        public override string ToString()
        {
            return "pi";
        }
    }
    public class OpE : Expression
    {
        public OpE()
        {
        }
        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
        public override bool IsEquivalent(Expression v)
        {
            if (v is OpE)
            {
                return true;
            }
            return false;
        }
        public override string ToString()
        {
            return "e";
        }
    }
    public class OpParam : Expression
    {
        public readonly string Name;
        public OpParam(string jmeno)
        {
            this.Name = jmeno;
        }

        public override T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);

        public override bool IsEquivalent(Expression v)
        {
            if (v is OpParam)
            {
                return Name == (v as OpParam).Name;
            }
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
