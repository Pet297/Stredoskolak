using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    public class Equation
    {
        public readonly Expression LeftSide;
        public readonly Expression RightSide;

        public Equation(Expression leftSide, Expression rightSide)
        {
            LeftSide = leftSide;
            RightSide = rightSide;
        }

        public HashSet<string> ListVariables()
        {
            HashSet<string> hs = LeftSide.ListVariables();
            hs.UnionWith(RightSide.ListVariables());
            return hs;
        }

        public Equation Clone()
        {
            return new Equation(LeftSide.Clone(), RightSide.Clone());
        }
        public Equation SubstitutingCopy(Dictionary<string, Expression> substitutions)
        {
            return new Equation(LeftSide.SubstitutingCopy(substitutions), RightSide.SubstitutingCopy(substitutions));
        }
        public Equation SimplifyingCopy()
        {
            return new Equation(LeftSide.SimplifyingCopy(), RightSide.SimplifyingCopy());
        }
        public EquationOperation IsolateVariableStep(string variable)
        {
            // Figure out on which side is the variable and what to do to isolate it

            bool right = RightSide.ListVariables().Contains(variable);
            bool left = LeftSide.ListVariables().Contains(variable);

            if (left && !right)
            {
                return new EquationOperation(EquationOperationType.swapSides, null, null);
            }
            else if (right && !left)
            {
                return RightSide.IsolateVariable(variable);
            }

            // The variable is on both sides. Use a special action.
            else
            {
                // Remove same power from both sides
                if (LeftSide is OpPower powl && RightSide is OpPower powr && powl.OperandExponent.IsEquivalent(powr.OperandExponent))
                {
                    return new EquationOperation(EquationOperationType.root, powr.OperandExponent, powr.OperandBase);
                }
                // Log both sides
                else if (LeftSide is OpPower powl1 && RightSide is OpPower powr1 && powl1.OperandBase.IsEquivalent(powr1.OperandBase))
                {
                    return new EquationOperation(EquationOperationType.logarithm, powr1.OperandBase, powr1.OperandExponent);
                }
                // Remove same log from both sides
                else if (LeftSide is OpLogarhithm logl && RightSide is OpLogarhithm logr && logl.OperandBase.IsEquivalent(logr.OperandBase))
                {
                    return new EquationOperation(EquationOperationType.powerFromLeft, logr.OperandBase, logr.OperandArgument);
                }
                // If both fails, subtract one side from the other
                else
                {
                    return new EquationOperation(EquationOperationType.minus, RightSide, new OpRational(0, 1));
                }
            }
        }
        public List<Equation> IsolateVariableApply(string variable, EquationOperation operation)
        {
            // polynomials can have many solutions and solving them could fail in many ways.
            if (operation.OperationType == EquationOperationType.evalPolynomial)
            {
                Expression right = RightSide.SimplifyingCopy();
                OpVariable checker = new OpVariable(variable);

                // Keep track of all coefficients for all integer powers
                Dictionary<int, List<Expression>> coeffs = new Dictionary<int, List<Expression>>
                {
                    { 0, new List<Expression>(){new OpAdditiveInverse(LeftSide) } }
                };

                int maxDegree = 0;
                int minDegree = 0;

                // Right side of the equation must be a sum, otherwise, different method of solving would be used.
                if (right is OpSum a)
                {
                    foreach (Expression exp in a.Operands)
                    {
                        // Keep track of integer power
                        int ct = 0;
                        // Keep track of multiplied expressions in the coefficient
                        List<Expression> expressions = new List<Expression>();

                        // The coefficient is a product of multiple terms
                        if (exp is OpProduct)
                        {
                            foreach (Expression exp2 in (exp as OpProduct).Operands)
                            {
                                if (exp2.IsEquivalent(checker)) ct++;
                                else if (exp2 is OpPower pow)
                                {
                                    if (pow.OperandBase.IsEquivalent(checker) && pow.OperandExponent is OpRational rat)
                                    {
                                        if (rat.Denominator == 1)
                                        {
                                            ct += (int)rat.Numerator;
                                        }
                                        else throw new SolverException("Couldn't evaluate sum containing multiple different non-integer or variable powers of a variable.");
                                    }
                                    else throw new SolverException("Couldn't evaluate sum containing multiple different non-integer or variable powers of a variable.");
                                }
                                else if (exp2.ListVariables().Contains(variable)) throw new SolverException("Couldn't evaluate sum containing powers of a variable, which isn't a polynomial of the given variable.");
                                else expressions.Add(exp2);
                            }
                        }
                        // The coefficient is a single term
                        else
                        {
                            if (exp.IsEquivalent(checker)) ct++;
                            else if (exp is OpPower pow)
                            {
                                if (pow.OperandBase.IsEquivalent(checker) && pow.OperandExponent is OpRational rat)
                                {
                                    if (rat.Denominator == 1)
                                    {
                                        ct += (int)rat.Numerator;
                                    }
                                    else throw new SolverException("Couldn't evaluate sum containing multiple different non-integer or variable powers of a variable.");
                                }
                                else throw new SolverException("Couldn't evaluate sum containing multiple different non-integer or variable powers of a variable.");
                            }
                            else if (exp.ListVariables().Contains(variable)) throw new SolverException("Couldn't evaluate sum containing powers of a variable, which isn't a polynomial of the given variable.");
                            else expressions.Add(exp);
                        }

                        if (ct > maxDegree) maxDegree = ct;
                        if (ct < minDegree) minDegree = ct;

                        if (!coeffs.ContainsKey(ct)) coeffs.Add(ct, new List<Expression>());
                        coeffs[ct].Add(new OpProduct(expressions.ToArray()));
                    }
                }

                List<Expression> solutions = new List<Expression>();

                // If there is a negative power, simulate multiplication of both sides of the equation with a positve power
                if (minDegree < 0) maxDegree -= minDegree;

                Dictionary<int, Expression> coeffs2 = new Dictionary<int, Expression>();
                foreach (KeyValuePair<int, List<Expression>> kp in coeffs)
                {
                    coeffs2.Add(kp.Key - minDegree, new OpSum(kp.Value.ToArray()).SimplifyingCopy());
                }

                // In case the degree is too large, try to facorize the polynomial by guessing
                while (maxDegree > Settings.MaxPolynomialDegree)
                {
                    while (maxDegree > Settings.MaxPolynomialDegree)
                    {
                        Expression solution = Simplification.GuessSolution(this, variable);
                        if (solution != null)
                        {
                            solutions.Add(solution);
                            Tuple<int, Equation> t = Simplification.ReducePolynomial(coeffs2, solution, maxDegree, variable);
                            maxDegree = t.Item1;
                        }
                        else break;
                    }

                    // We could't guess the solution
                    if (maxDegree > Settings.MaxPolynomialDegree && solutions.Count == 0) throw new SolverException($"Wasn't able to calculate polynomial of degree {Settings.MaxPolynomialDegree + 1} or more by guessing.");
                }

                // Use fromula if/once possible
                if (maxDegree <= Settings.MaxPolynomialDegree)
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        if (!coeffs2.ContainsKey(i)) coeffs2.Add(i, new OpRational(0, 1));
                    }

                    Expression c4 = coeffs2[4];
                    Expression c3 = coeffs2[3];
                    Expression c2 = coeffs2[2];
                    Expression c1 = coeffs2[1];
                    Expression c0 = coeffs2[0];

                    if (c4.IsEquivalent(new OpRational(0, 1)))
                    {
                        if (c3.IsEquivalent(new OpRational(0, 1)))
                        {
                            if (c2.IsEquivalent(new OpRational(0, 1)))
                            {
                                if (c1.IsEquivalent(new OpRational(0, 1)))
                                {
                                    //DEGREE 0
                                    //solutions.AddRange(Simplification.CalculatePolynomial(c0));
                                }
                                else
                                {
                                    //DEGREE 1
                                    solutions.AddRange(Simplification.CalculatePolynomial(c0, c1));
                                }
                            }
                            else
                            {
                                //DEGREE 2
                                solutions.AddRange(Simplification.CalculatePolynomial(c0, c1, c2));
                            }
                        }
                        else
                        {
                            //DEGREE 3
                            solutions.AddRange(Simplification.CalculatePolynomial(c0, c1, c2, c3));
                        }
                    }
                    else
                    {
                        //DEGREE 4
                        solutions.AddRange(Simplification.CalculatePolynomial(c0, c1, c2, c3, c4));
                    }

                    if (solutions.Count == 0) throw new SolverException("Polynomial has no solutions.");
                }

                // Return all solutions
                List<Equation> leq = new List<Equation>();
                foreach (Expression ex in solutions) leq.Add(new Equation(new OpVariable(variable), ex));
                return leq;
            }

            // trigs calculated by table can have up to 2 (parametrized) solutions.
            else if (operation.OperationType == EquationOperationType.evalTrigTable)
            {
                // Figure out on which side is the trig function, then use table to find 1 or 2 roots
                // If table fails, just apply arccos, or arcsin and do not calculate the value

                if (LeftSide is OpSin asinl)
                {
                    List<Expression> solutions = Simplification.LookUpArcSin(RightSide.SimplifyingCopy());
                    List<Equation> newEquations = new List<Equation>();

                    if (solutions.Count == 0) return new List<Equation>() { new Equation(new OpArcSin(RightSide), asinl.Operand) };

                    foreach (Expression e in solutions)
                    {
                        newEquations.Add(new Equation(e, asinl.Operand));
                    }
                    return newEquations;
                }
                else if (LeftSide is OpCos acosl)
                {
                    List<Expression> solutions = Simplification.LookUpArcCos(RightSide.SimplifyingCopy());
                    List<Equation> newEquations = new List<Equation>();

                    if (solutions.Count == 0) return new List<Equation>() { new Equation(new OpArcCos(RightSide), acosl.Operand) };

                    foreach (Expression e in solutions)
                    {
                        newEquations.Add(new Equation(e, acosl.Operand));
                    }
                    return newEquations;
                }
                else if (RightSide is OpSin asinr)
                {
                    List<Expression> solutions = Simplification.LookUpArcSin(LeftSide.SimplifyingCopy());
                    List<Equation> newEquations = new List<Equation>();

                    if (solutions.Count == 0) return new List<Equation>() { new Equation(asinr.Operand, new OpArcSin(LeftSide)) };

                    foreach (Expression e in solutions)
                    {
                        newEquations.Add(new Equation(asinr.Operand, e));
                    }
                    return newEquations;
                }
                else if (RightSide is OpCos acosr)
                {
                    List<Expression> solutions = Simplification.LookUpArcCos(LeftSide.SimplifyingCopy());
                    List<Equation> newEquations = new List<Equation>();

                    if (solutions.Count == 0) return new List<Equation>() { new Equation(acosr.Operand, new OpArcSin(LeftSide)) };

                    foreach (Expression e in solutions)
                    {
                        newEquations.Add(new Equation(acosr.Operand, e));
                    }
                    return newEquations;
                }
            }

            // swapping sides is considered a special operation.
            else if (operation.OperationType == EquationOperationType.swapSides)
            {
                return new List<Equation>() { new Equation(RightSide.Clone(), LeftSide.Clone()) };
            }

            // doing nothing is too considered a special operation.
            else if (operation.OperationType == EquationOperationType.none)
            {
                return new List<Equation>() { new Equation(LeftSide, RightSide) };
            }
            
            // generic equation operations are applied to both sides.
            return new List<Equation>() { new Equation(operation.ApplyToExpression(LeftSide), operation.NewValueOfOneSide) };
        }

        public static bool TestSolution(List<Equation> system, Dictionary<string, Expression> variables)
        {
            // See if a supposed solution reduces all equations to 0=0

            bool allTrue = true;

            foreach (Equation equation in system)
            {
                Equation e2 = equation.SubstitutingCopy(variables);
                Equation e3 = e2.SimplifyingCopy();

                if (!e3.LeftSide.IsEquivalent(e3.RightSide))
                {
                    allTrue = false;
                    break;
                }
            }

            return allTrue;
        }

        public bool IsVariableIsolated
        {
            // Either there is an isolated variable on one side, or there are none at all.
            get
            {
                if ((RightSide is OpVariable && !LeftSide.ListVariables().Contains((RightSide as OpVariable).Name))||
                    (LeftSide is OpVariable && !RightSide.ListVariables().Contains((LeftSide as OpVariable).Name)) ||
                    RightSide.IsRealNumber() && LeftSide.IsRealNumber()) return true;
                return false;
            }
        }
        public Tuple<string, Expression> GetCalculatedValue()
        {
            if (RightSide is OpVariable vr && !LeftSide.ListVariables().Contains(vr.Name)) return new Tuple<string, Expression>((RightSide as OpVariable).Name, LeftSide);
            else if (LeftSide is OpVariable vl && !RightSide.ListVariables().Contains(vl.Name)) return new Tuple<string, Expression>((LeftSide as OpVariable).Name, RightSide);
            else if (LeftSide.IsRealNumber() && RightSide.IsRealNumber())
            {
                // The is no calculated value, the equation is false, or true.
                return null;
            }
            throw new SolverException("Solver wasn't able to isolate a variable.");
        }

        public override string ToString()
        {
            return LeftSide.ToString() + "=" + RightSide.ToString();
        }
    }
}
