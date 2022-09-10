using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    public static class Simplification
    {
        // Calculating polynomials from formula. 3rd and 4th degree aren't supported.
        public static List<Expression> CalculatePolynomial(Expression coef0)
        {
            if (coef0.IsEquivalent(new OpRational(0, 1)))
            {
                throw new SolverException("Infinitely many solutions");
            }
            else
            {
                // 0 = 1
                return new List<Expression>();
            }
        }
        public static List<Expression> CalculatePolynomial(Expression coef0, Expression coef1)
        {
            return new List<Expression>()
            {
                // -b / a
                new OpProduct(new OpAdditiveInverse(coef0), new OpMultiplicativeInverse(coef1))
            };
        }
        public static List<Expression> CalculatePolynomial(Expression coef0, Expression coef1, Expression coef2)
        {
            return new List<Expression>()
            {
                 // -b + sqrt(b*b-4ac) / 2a
                 new OpProduct(
                 new OpSum(new OpAdditiveInverse(coef1.Clone()), new OpRoot(new OpRational(2, 1), new OpSum(new OpProduct(coef1.Clone(), coef1.Clone()), new OpProduct(new OpRational(-4, 1), coef0.Clone(), coef2.Clone())))),
                 new OpMultiplicativeInverse(new OpProduct(new OpRational(2, 1), coef2.Clone()))),

                 // -b - sqrt(b*b-4ac) / 2a
                 new OpProduct(
                 new OpSum(new OpAdditiveInverse(coef1.Clone()), new OpAdditiveInverse(new OpRoot(new OpRational(2, 1), new OpSum(new OpProduct(coef1, coef1), new OpProduct(new OpRational(-4, 1), coef0.Clone(), coef2.Clone()))))),
                 new OpMultiplicativeInverse(new OpProduct(new OpRational(2, 1), coef2.Clone()))),

            };
        }
        public static List<Expression> CalculatePolynomial(Expression coef0, Expression coef1, Expression coef2, Expression coef3)
        {
            //The commentated code doesn't seem to produce right results
            /*Expression p = new OpAdditiveInverse(new OpTimesGroup(coef2.Copy(), new OpMultiplicativeInverse(coef3.Copy()), new OpRational(1, 3))).SimplifyingCopy();
            Expression q = new OpAddGroup(new OpPower(p.Copy(),new OpRational(3,1)), new OpTimesGroup(new OpAddGroup(new OpTimesGroup(coef1.Copy(), coef2.Copy()),new OpAdditiveInverse(new OpTimesGroup(new OpRational(3,1),coef0.Copy(), coef3.Copy()))),new OpMultiplicativeInverse(new OpTimesGroup(new OpRational(6,1), new OpPower(coef3.Copy() ,new OpRational(2,1)))))).SimplifyingCopy();
            Expression r = new OpTimesGroup(coef1.Copy(), new OpMultiplicativeInverse(coef3.Copy()), new OpRational(1, 3)).SimplifyingCopy();
            Expression s = new OpPower(new OpAddGroup(new OpPower(q.Copy(), new OpRational(2, 1)),new OpPower(new OpAddGroup(r.Copy(), new OpPower(p.Copy(), new OpRational(2, 1))), new OpRational(3, 1))), new OpRational(1, 2)).SimplifyingCopy();

            return new List<Expression>()
            {
                new OpAddGroup(
                    new OpPower(new OpAddGroup(q.Copy(),s.Copy()),new OpRational(1,3)),
                    new OpPower(new OpAddGroup(q.Copy(),new OpAdditiveInverse(s.Copy())),new OpRational(1,3)),
                    p.Copy()
                ).SimplifyingCopy()
            };*/
            throw new SolverException("Solving 3rd degree polynomials by formula isn't supported yet.");
        }
        public static List<Expression> CalculatePolynomial(Expression coef0, Expression coef1, Expression coef2, Expression coef3, Expression coef4)
        {
            throw new SolverException("Solving 4th degree polynomials by formula isn't supported yet.");
        }

        /// <summary>
        /// Determines wheter an expression is a rational multiple of pi
        /// If so returns the rational, othervise returns null
        /// </summary>
        /// <param name="reducedExpression"></param>
        /// <returns>null or a rational number</returns>
        public static OpRational IsRationalMultipleOfPi(Expression reducedExpression)
        {
            if (reducedExpression is OpPi)
            {
                return new OpRational(1, 1);
            }
            else if (reducedExpression is OpRational rat)
            {
                if (rat.IsEquivalent(new OpRational(0, 1))) return rat;
            }
            else if (reducedExpression is OpProduct times)
            {
                if (times.Operands.Count == 2)
                {
                    if (times.Operands[0] is OpPi && times.Operands[1] is OpRational) return times.Operands[1] as OpRational;
                    else if (times.Operands[1] is OpPi && times.Operands[0] is OpRational) return times.Operands[0] as OpRational;
                }
                else return null;
            }

            return null;
        }

        public static Expression LookUpSin(OpRational rationalMultipleOfPi)
        {
            if (12 % rationalMultipleOfPi.Denominator == 0)
            {
                int mult = (int)((rationalMultipleOfPi.Numerator * (12 / rationalMultipleOfPi.Denominator)) % 24);
                mult = (mult + 24) % 24;

                switch(mult)
                {
                    case 0: return new OpRational(0, 1);
                    case 2: return new OpRational(1, 2);
                    case 3: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(1, 2));
                    case 4: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(1, 2));
                    case 6: return new OpRational(1, 1);
                    case 8: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(1, 2));
                    case 9: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(1, 2));
                    case 10: return new OpRational(1, 2);
                    case 12: return new OpRational(0, 1);
                    case 14: return new OpRational(-1, 2);
                    case 15: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(-1, 2));
                    case 16: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(-1, 2));
                    case 18: return new OpRational(-1, 1);
                    case 20: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(-1, 2));
                    case 21: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(-1, 2));
                    case 22: return new OpRational(-1, 2);
                }
            }

            return null;
        }
        public static Expression LookUpCos(OpRational rationalMultipleOfPi)
        {
            if (12 % rationalMultipleOfPi.Denominator == 0)
            {
                int mult = (int)((rationalMultipleOfPi.Numerator * (12 / rationalMultipleOfPi.Denominator)) % 24);
                mult = (mult + 24) % 24;

                switch (mult)
                {
                    case 0: return new OpRational(1, 1);
                    case 2: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(1, 2));
                    case 3: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(1, 2));
                    case 4: return new OpRational(1, 2);
                    case 6: return new OpRational(0, 1);
                    case 8: return new OpRational(-1, 2);
                    case 9: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(-1, 2));
                    case 10: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(-1, 2));
                    case 12: return new OpRational(-1, 1);
                    case 14: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(-1, 2));
                    case 15: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(-1, 2));
                    case 16: return new OpRational(-1, 2);
                    case 18: return new OpRational(0, 1);
                    case 20: return new OpRational(1, 2);
                    case 21: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(2, 1)), new OpRational(1, 2));
                    case 22: return new OpProduct(new OpRoot(new OpRational(2, 1), new OpRational(3, 1)), new OpRational(1, 2));
                }
            }

            return null;
        }
        public static List<Expression> LookUpArcSin(Expression simplifiedExpression)
        {
            if (simplifiedExpression is OpRational)
            {
                if (simplifiedExpression.IsEquivalent(new OpRational(0, 1))) return new List<Expression>() { new OpProduct(new OpPi(), new OpParam("k")) };
                else if (simplifiedExpression.IsEquivalent(new OpRational(1, 2))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(1,6))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(5,6))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpRational(1, 1))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(1,2)))
                };
                else if (simplifiedExpression.IsEquivalent(new OpRational(-1, 1))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(3,2)))
                };
                else if (simplifiedExpression.IsEquivalent(new OpRational(-1, 2))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(7,6))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(11,6))),
                };
            }
            else if (simplifiedExpression is OpProduct)
            {
                if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(1, 2), new OpPower(new OpRational(2, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(1, 4))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(3, 4))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(1, 2), new OpPower(new OpRational(3, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(1, 3))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(2, 3))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(-1, 2), new OpPower(new OpRational(2, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(5, 4))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(7, 4))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(-1, 2), new OpPower(new OpRational(3, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(4, 3))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(5, 3))),
                };
            }

            return new List<Expression>();
        }
        public static List<Expression> LookUpArcCos(Expression simplifiedExpression)
        {
            if (simplifiedExpression is OpRational)
            {
                if (simplifiedExpression.IsEquivalent(new OpRational(0, 1))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k")),
                    new OpProduct(new OpPi(), new OpRational(1,2)))
                };
                else if (simplifiedExpression.IsEquivalent(new OpRational(1, 2))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(1,3))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(5,3))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpRational(1, 1))) return new List<Expression>() {
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1))
                };
                else if (simplifiedExpression.IsEquivalent(new OpRational(-1, 1))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpPi())
                };
                else if (simplifiedExpression.IsEquivalent(new OpRational(-1, 2))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(2,3))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2,1)),
                    new OpProduct(new OpPi(), new OpRational(4,3))),
                };
            }
            else if (simplifiedExpression is OpProduct)
            {
                if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(1, 2), new OpPower(new OpRational(2, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(1, 4))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(7, 4))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(1, 2), new OpPower(new OpRational(3, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(1, 6))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(11, 6))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(-1, 2), new OpPower(new OpRational(2, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(3, 4))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(5, 4))),
                };
                else if (simplifiedExpression.IsEquivalent(new OpProduct(new OpRational(-1, 2), new OpPower(new OpRational(3, 1), new OpRational(1, 2))))) return new List<Expression>() {
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(5, 6))),
                    new OpSum(
                    new OpProduct(new OpPi(), new OpParam("k"), new OpRational(2, 1)),
                    new OpProduct(new OpPi(), new OpRational(7, 6))),
                };
            }

            return new List<Expression>();
        }
        /// <summary>
        /// Guesses the solution based on settings. Returns first one found.
        /// If none is found, returns null
        /// </summary>
        /// <param name="eq">Equation to guess root of.</param>
        /// <param name="variable">Variable to guess value of.</param>
        /// <returns>Found solution as expression, or null if none is found.</returns>
        public static Expression GuessSolution(Equation eq, string variable)
        {
            foreach (Expression e in Settings.GetGuessedNumbers())
            {
                Equation eq2 = eq.SubstitutingCopy(new Dictionary<string, Expression>() { { variable, e} });
                eq2 = eq2.SimplifyingCopy();

                if (eq2.LeftSide.IsEquivalent(eq2.RightSide)) return e;
            }
            return null;
        }
        /// <summary>
        /// Reduces sparsely represented polynomial, after a solution is found to decrease its degree.
        /// </summary>
        /// <param name="coefs">Sparse list of coefficients, where keys are the powers and values are the coefficients.</param>
        /// <param name="solution">Known solution to the polynomial to use in reduction.</param>
        /// <param name="maxDegree">Current degree of the polynomial.</param>
        /// <param name="variableName">Name of the variable which the polynomial is of of.</param>
        /// <returns>New max degree after reduction and new polynomial equation after reduction.</returns>
        public static Tuple<int, Equation> ReducePolynomial(Dictionary<int, Expression> coefs, Expression solution, int maxDegree, string variableName)
        {
            Dictionary<int, Expression> newCoefs = new Dictionary<int, Expression>();
            for (int i = maxDegree; i > 0; i--)
            {
                if (coefs.ContainsKey(i))
                {
                    Expression count = new OpProduct(coefs[i], solution).SimplifyingCopy();
                    newCoefs.Add(i - 1, count);
                    coefs.Remove(i);

                    if (coefs.ContainsKey(i-1))
                    {
                        coefs[i - 1] = new OpSum(coefs[i - 1].Clone(), count).SimplifyingCopy();
                        if (coefs[i - 1].IsEquivalent(new OpRational(0, 1))) coefs.Remove(i - 1);
                    }
                    else
                    {
                        coefs.Add(i - 1, count.SimplifyingCopy());
                    }
                }
            }
            if (coefs.ContainsKey(0)) throw new SolverException($"{solution} isn't a solution to the given polynomial.");

            int newMaxDegree = 0;
            OpSum leftSide = new OpSum();
            Expression rightSide = new OpRational(0, 1);
            foreach (KeyValuePair<int, Expression> kp in newCoefs)
            {
                coefs.Add(kp.Key, kp.Value);
                if (kp.Key > newMaxDegree) newMaxDegree = kp.Key;

                if (kp.Key == 0)
                {
                    rightSide = new OpAdditiveInverse(kp.Value).SimplifyingCopy();
                }

                else
                {
                    OpProduct term = new OpProduct();
                    for (int i = 0; i < kp.Key; i++)
                    {
                        term.Operands.Add(new OpVariable(variableName));
                    }
                    term.Operands.Add(kp.Value);

                    leftSide.Operands.Add(term.SimplifyingCopy());
                }
            }

            return new Tuple<int, Equation>(newMaxDegree, new Equation(leftSide, rightSide));
        }

        /// <summary>
        /// Given a sum of rational multiples of expressions, add another term, either by adding new entry if it's unique,
        /// or changing a multiplier if it's alredy in the sum.
        /// Use it to sum identical terms in a sum.
        /// </summary>
        /// <param name="terms">List of rational multiples of expressions.</param>
        /// <param name="term">Term to add to the sum.</param>
        public static void AddNonrationalTerm(List<Tuple<OpRational, Expression>> terms, Expression term)
        {
            Expression toAdd;
            OpRational count;

            if (term is OpProduct otg)
            {
                count = new OpRational(1, 1);
                OpProduct newTg = otg.Clone() as OpProduct;

                foreach(Expression e in newTg.Operands)
                {
                    if (e is OpRational e2)
                    {
                        count = e2;
                        newTg.Operands.Remove(e);
                        break;
                    }
                }

                if (newTg.Operands.Count == 1) toAdd = newTg.Operands[0];
                else toAdd = newTg;
            }
            else if (term is OpAdditiveInverse a)
            {
                count = new OpRational(-1, 1);
                toAdd = a.Operand;
            }
            else
            {
                count = new OpRational(1, 1);
                toAdd = term;
            }

            //Add to terms
            bool found = false;
            foreach (Tuple<OpRational, Expression> t in terms)
            {
                if (t.Item2.IsEquivalent(toAdd))
                {
                    found = true;
                    terms.Remove(t);

                    OpRational newCount = t.Item1 + count;

                    if (!newCount.IsEquivalent(new OpRational(0, 1))) terms.Add(new Tuple<OpRational, Expression>(newCount, toAdd));

                    break;
                }
            }
            if (!found) terms.Add(new Tuple<OpRational, Expression>(count, toAdd));
        }
        /// <summary>
        /// Given a product of arbitrary powers of expressions, add another factor, either by adding new entry if it's unique,
        /// or changing a power if it's alredy in the product.
        /// Use it to multiply together identical terms in a product.
        /// </summary>
        /// <param name="factors">List of powers (first Expression in tuple) of expressions (second one).</param>
        /// <param name="factor">Factor to add to the product.</param>
        public static void AddNonrationalFactor(List<Tuple<Expression, Expression>> factors, Expression factor)
        {
            Expression toAdd;
            Expression power;

            if (factor is OpPower pow)
            {
                power = pow.OperandExponent;
                toAdd = pow.OperandBase;
            }
            else if (factor is OpMultiplicativeInverse a)
            {
                power = new OpRational(-1, 1);
                toAdd = a.Operand;
            }
            else
            {
                power = new OpRational(1, 1);
                toAdd = factor;
            }

            //Add to factors
            bool found = false;
            foreach (Tuple<Expression, Expression> t in factors)
            {
                if (t.Item2.IsEquivalent(toAdd))
                {
                    found = true;
                    factors.Remove(t);

                    Expression newCount = new OpSum(power,t.Item1).SimplifyingCopy();

                    if (!newCount.IsEquivalent(new OpRational(0, 1))) factors.Add(new Tuple<Expression, Expression>(newCount, toAdd));

                    break;
                }
            }
            if (!found) factors.Add(new Tuple<Expression, Expression>(power, toAdd));
        }

        /// <summary>
        /// Gived a sum of logs with varisous bases, connect them all together by multipling their arguments.
        /// Use it to add together logs with identical bases.
        /// </summary>
        /// <param name="logarhithms">List of logarithms with their bases, and their arguments.</param>
        /// <param name="log">Logarithm to add.</param>
        /// <returns>False if added logarithm was unique, true othervise.</returns>
        public static bool AddLogarhithm(List<Tuple<Expression, Expression>> logarhithms, OpLogarhithm log)
        {
            foreach(Tuple<Expression, Expression> t in logarhithms)
            {
                if (t.Item1.IsEquivalent(log.OperandBase))
                {
                    (t.Item2 as OpProduct).Operands.Add(log.OperandArgument.Clone());
                    return true;
                }
            }

            logarhithms.Add(new Tuple<Expression, Expression>(log.OperandBase.Clone(), new OpProduct(log.OperandArgument.Clone())));
            return false;
        }
        /// <summary>
        /// Given product of powers with various bases, connetct them all together, by summing the powers.
        /// </summary>
        /// <param name="powers">List of bases and their powers.</param>
        /// <param name="pow">Power to add to the product</param>
        /// <returns>False if added power was unique, true othervise.</returns>
        public static bool AddPower(List<Tuple<Expression, Expression>> powers, OpPower pow)
        {
            foreach (Tuple<Expression, Expression> t in powers)
            {
                if (t.Item1.IsEquivalent(pow.OperandBase))
                {
                    (t.Item2 as OpSum).Operands.Add(pow.OperandExponent.Clone());
                    return true;
                }
            }

            powers.Add(new Tuple<Expression, Expression>(pow.OperandBase.Clone(), new OpSum(pow.OperandExponent.Clone())));
            return false;
        }
        /// <summary>
        /// Given product of powers with various bases, connetct them all together, by summing the powers.
        /// Adds given expression which isn't a power, as a first power.
        /// </summary>
        /// <param name="powers">List of bases and their powers.</param>
        /// <param name="pow">Expression to add.</param>
        /// <returns>False if added expression was unique, true othervise.</returns>
        public static bool AddFirstPower(List<Tuple<Expression, Expression>> powers, Expression pow)
        {
            foreach (Tuple<Expression, Expression> t in powers)
            {
                if (t.Item1.IsEquivalent(pow))
                {
                    (t.Item2 as OpSum).Operands.Add(new OpRational(1, 1));
                    return true;
                }
            }

            powers.Add(new Tuple<Expression, Expression>(pow.Clone(), new OpSum(new OpRational(1, 1))));
            return false;
        }

        public static Expression FullSimplification(Expression original)
        {
            while (true)
            {
                Expression e1 = original.Clone();
                original = original.SimplifyingCopy();

                if (e1.IsEquivalent(original)) return e1;
            }
        }
    }
}
