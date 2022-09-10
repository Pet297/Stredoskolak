using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Stredoskolak
{
    public static class NumberEnumerators
    {
        public static IEnumerable<Expression> RealExpressions
        {
            get
            {
                IEnumerator<Expression> enum0 = Integers.GetEnumerator();
                IEnumerator<Expression> enum1 = NonIntegerRationals.GetEnumerator();
                IEnumerator<Expression> enum2 = SimpleRoots.GetEnumerator();
                IEnumerator<Expression> enum3 = AllSums.GetEnumerator();

                int e0ct = 0;
                int e1ct = 0;
                int e2ct = 0;

                while (true)
                {
                    enum0.MoveNext();
                    yield return enum0.Current;
                    e0ct++;

                    if (e0ct == Settings.GuessedNumberComplexity)
                    {
                        e0ct = 0;
                        enum1.MoveNext();
                        yield return enum1.Current;
                        e1ct++;
                    }

                    if (e1ct == Settings.GuessedNumberComplexity)
                    {
                        e1ct = 0;
                        enum2.MoveNext();
                        yield return enum2.Current;
                        e2ct++;
                    }

                    if (e2ct == Settings.GuessedNumberComplexity)
                    {
                        e2ct = 0;
                        enum3.MoveNext();
                        yield return enum3.Current;
                    }
                }
            }
        }
        public static IEnumerable<Expression> IrationalExpressions
        {
            get
            {
                IEnumerator<Expression> enum0 = SimpleRoots.GetEnumerator();
                IEnumerator<Expression> enum1 = AllSums.GetEnumerator();
                IEnumerator<Expression> enum2 = SimpleOperators.GetEnumerator();

                int e0ct = 0;

                while (true)
                {
                    enum0.MoveNext();
                    yield return enum0.Current;
                    e0ct++;

                    if (e0ct == Settings.GuessedNumberComplexity)
                    {
                        e0ct = 0;
                        enum1.MoveNext();
                        yield return enum1.Current;

                        enum2.MoveNext();
                        yield return enum2.Current;
                    }
                }
            }
        }

        public static IEnumerable<Expression> Integers
        {
            get
            {
                yield return new OpRational(0, 1);

                for (BigInteger num = 1; ; num++)
                {
                    yield return new OpRational(num, 1);
                    yield return new OpRational(-num, 1);
                }
            }
        }
        public static IEnumerable<Expression> NonIntegerRationals
        {
            get
            {
                for (BigInteger sum = 1; ; sum++)
                {
                    for (BigInteger num = 1; num < sum - 1; num++)
                    {
                        if (BigInteger.GreatestCommonDivisor(num, sum - num) == 1)
                        {
                            yield return new OpRational(num, sum - num);
                            yield return new OpRational(-num, sum - num);
                        }
                    }
                }
            }
        }
        public static IEnumerable<Expression> Rationals
        {
            get
            {
                yield return new OpRational(0, 1);

                for (BigInteger sum = 1; ; sum++)
                {
                    for (BigInteger num = 1; num < sum; num++)
                    {
                        if (BigInteger.GreatestCommonDivisor(num, sum-num)==1)
                        {
                            yield return new OpRational(num, sum - num);
                            yield return new OpRational(-num, sum - num);
                        }
                    }
                }
            }
        }

        public static IEnumerable<Expression> SimpleRoots
        {
            get
            {
                for (int i = 1; ; i++)
                {
                    IEnumerator<Expression> e2 = RealExpressions.GetEnumerator(); // base
                    e2.MoveNext();
                    e2.MoveNext();

                    for (int j = 0; j < i-1; j++)
                    {
                        for (int k = 0; k < Settings.GuessedNumberComplexity; k++)
                        {
                            e2.MoveNext();
                            yield return new OpRoot(new OpRational(1 + i, 1), e2.Current);
                            yield return new OpAdditiveInverse(new OpRoot(new OpRational(1 + i, 1), e2.Current));
                        }
                    }

                    for (int k = 0; k < Settings.GuessedNumberComplexity; k++)
                    {

                        for (int j = 2; j < i + 2; j++)
                        {
                            e2.MoveNext();
                            yield return new OpRoot(new OpRational(j, 1), e2.Current);
                            yield return new OpAdditiveInverse(new OpRoot(new OpRational(1 + i, 1), e2.Current));
                        }
                    }
                }
            }
        }
        public static IEnumerable<Expression> SimpleOperators
        {
            get
            {
                yield return new OpPi();
                yield return new OpE();


                IEnumerator<Expression> reals = RealExpressions.GetEnumerator();

                while (true)
                {
                    reals.MoveNext();

                    yield return new OpSin(reals.Current);
                    yield return new OpCos(reals.Current);
                    yield return new OpTg(reals.Current);
                    yield return new OpCotg(reals.Current);
                    yield return new OpSec(reals.Current);
                    yield return new OpCsc(reals.Current);

                    yield return new OpArcSin(reals.Current);
                    yield return new OpArcCos(reals.Current);
                    yield return new OpArcTg(reals.Current);
                    yield return new OpArcCotg(reals.Current);
                    yield return new OpArcSec(reals.Current);
                    yield return new OpArcCsc(reals.Current);
                }
            }
        }
        public static IEnumerable<Expression> AllSums
        {
            get
            {
                for (int i = 1; ; i++)
                {
                    IEnumerator<Expression> er = Rationals.GetEnumerator();
                    IEnumerator<Expression> ei = IrationalExpressions.GetEnumerator();

                    // Don't add 0.
                    er.MoveNext();

                    for (int j = 0; j < i; j++) ei.MoveNext();

                    for (int j = 0; j < i; j++)
                    {
                        er.MoveNext();
                        yield return new OpSum(er.Current, ei.Current);
                    }

                    ei = IrationalExpressions.GetEnumerator();

                    for (int j = 0; j < i - 1; j++)
                    {
                        ei.MoveNext();
                        yield return new OpSum(er.Current, ei.Current);
                    }
                }
            }
        }
    }
}
