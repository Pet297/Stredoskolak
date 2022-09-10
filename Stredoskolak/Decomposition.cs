using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Stredoskolak
{
    public static class Decomposition
    {
        public static List<BigInteger> DecomposeNumber(BigInteger input)
        {
            return DecomposeNumber(input, Settings.MaxDecompositionFactor);
        }
        public static List<BigInteger> DecomposeNumber(BigInteger input, int maxFactor)
        {
            // Do not allow larger primes than the one in settings.
            if (maxFactor > Settings.MaxDecompositionFactor) maxFactor = Settings.MaxDecompositionFactor;

            // Special cases
            if (input == 0) return new List<BigInteger>() { 0 };
            if (input == 1) return new List<BigInteger>() { 1 };
            if (input == -1) return new List<BigInteger>() { -1 };

            List<BigInteger> factors = new List<BigInteger>();

            // For negative numbers, add -1 once to the output
            if (input < 0)
            {
                factors.Add(-1);
                input = -input;
            }

            // Factorize the positive integer
            foreach (int prime in Settings.GetPrimes(maxFactor))
            {
                while (input % prime == 0)
                {
                    factors.Add(new BigInteger(prime));
                    input /= prime;
                }
            }
            // Add (possibly non-prime) remaining number, which can't be factorized further without the knowledge of large enough prime numbers.
            if (input != 1) factors.Add(input);

            return factors;
        }

        public static (BigInteger, BigInteger) CancelFraction(BigInteger numerator, BigInteger denominator)
        {
            // Deal with 0/1, 1/0 and 0/0
            if (numerator == 0 && denominator == 0) return (0, 0);
            else if (denominator == 0) return (1, 0);
            else if (numerator == 0) return (0, 1);

            // If obvious, return 1/1 or -1/1
            if (numerator == denominator) return (1, 1);
            if (numerator == -denominator) return (-1, 1);

            // If result is negative, make numerator negative
            bool negative = numerator < 0 ^ denominator < 0;
            if (numerator < 0 ^ negative) numerator = -numerator;
            if (denominator < 0) denominator = -denominator;

            // Cancel factors up to considered factor and return result
            foreach (int prime in Settings.GetPrimes())
            {
                while (numerator % prime == 0 && denominator % prime == 0)
                {
                    numerator /= prime;
                    denominator /= prime;
                }
            }

            return (numerator, denominator);
        }

        public static (BigInteger, BigInteger) PartialRoot(int root, BigInteger radicant)
        {
            if (root < 0) root = -root;
            else if (root == 0) throw new SolverException("Square root exponent can't be zero.");

            if (radicant == 0) return (0, 0);

            if (root == 1) return (radicant, 1);

            BigInteger wholePart = 1;
            BigInteger radicalPart = 1;

            foreach (int prime in Settings.GetPrimes())
            {
                int ct = 0;
                while (radicant % prime == 0)
                {
                    ct++;
                    if (ct == root)
                    {
                        ct = 0;
                        wholePart *= prime;
                    }
                    radicant /= prime;
                }
                for (int i = 0; i < ct; i++) radicalPart *= prime;
            }
            radicalPart *= radicant;

            return (wholePart, radicalPart);
        }
        public static Expression PartialRootFull(int root, int power, BigInteger radicant)
        {
            // If evaluating the root would be too complex, don't do it.
            if (Settings.MaxSizeOfNumbersInBytes < radicant.GetByteCount() * (power % root))
            {
                return new OpPower(new OpRational(radicant, 1), new OpRational(power, root));
            }

            // If power exponent is larger than root exponent, then we divide them, keep the whole part as an integer power of input
            // and only deal with fractional part lesser than 1 while calculating the root.
            //
            // This is done to prevent unsafely large exponents as much as possible.
            OpProduct result = new OpProduct();
            if (power >= root)
            {
                OpPower wholePart = new OpPower(new OpRational(radicant, 1), new OpRational(power / root, 1));

                result.Operands.Add(wholePart);
            }
            int modulatedPower = power % root;

            // Decompose root to calculate partial roots
            List<BigInteger> decomposedRoot = DecomposeNumber(root);

            // When calculating partial roots, keep track of current root
            int rootLeft = root;
            BigInteger resultBuffer = BigInteger.Pow(radicant,modulatedPower);

            // At each step, decide which factors of the root will stay under the root and which ones can be taken out
            // Keep track of root applied to both of those
            foreach (BigInteger bi in decomposedRoot)
            {
                if (bi > Settings.MaxExponentFactorOfRoot)
                {
                    result.Operands.Add(new OpPower(new OpRational(resultBuffer, 1), new OpRational(1, rootLeft)));
                    return result;
                }

                (BigInteger, BigInteger) wholePart = PartialRoot((int)bi, resultBuffer);

                if (!wholePart.Item2.IsOne) result.Operands.Add(new OpPower(new OpRational(wholePart.Item2, 1), new OpRational(1, rootLeft)));
                rootLeft /= (int)bi;
                resultBuffer = wholePart.Item1;
            }

            if (!resultBuffer.IsOne) result.Operands.Add(new OpRational(resultBuffer, 1));

            return result;
        }

        public static (int, OpRational) GetPowerAndRemainder(OpRational factor, OpRational rational)
        {
            // This is used to calculate logs.
            
            // Decompose both input rationals - 4 decompositions in total
            List<BigInteger> numerator = DecomposeNumber(rational.Numerator);
            List<BigInteger> denominator = DecomposeNumber(rational.Denominator);
            List<BigInteger> factorN = DecomposeNumber(factor.Numerator);
            List<BigInteger> factorD = DecomposeNumber(factor.Denominator);

            // Deal with 1s in the decomposition
            if (factorN.Contains(1)) factorN.Remove(1);
            if (factorD.Contains(1)) factorD.Remove(1);

            List<BigInteger> factorNbuffer = new List<BigInteger>();
            List<BigInteger> factorDbuffer = new List<BigInteger>();

            int power = 0;

            // Will the result of the log be positive or negative?
            bool positive = false;
            foreach (BigInteger bi in factorN)
            {
                if (!denominator.Contains(bi))
                {
                    positive = true;
                    break;
                }
            }
            foreach (BigInteger bi in factorD)
            {
                if (!numerator.Contains(bi))
                {
                    positive = true;
                    break;
                }
            }
            if (!positive)
            {
                List<BigInteger> p = numerator;
                numerator = denominator;
                denominator = p;
            }

            // As long as possible, try to divide the input number by the base (by removing factors)
            while (true)
            {
                bool fail = false;
                while (factorN.Count > 0)
                {
                    if (numerator.Contains(factorN[0]))
                    {
                        numerator.Remove(factorN[0]);
                        factorNbuffer.Add(factorN[0]);
                        factorN.RemoveAt(0);
                    }
                    else
                    {
                        fail = true;
                        break;
                    }
                }
                while (factorD.Count > 0)
                {
                    if (denominator.Contains(factorD[0]))
                    {
                        denominator.Remove(factorD[0]);
                        factorDbuffer.Add(factorD[0]);
                        factorD.RemoveAt(0);
                    }
                    else
                    {
                        fail = true;
                        break;
                    }
                }

                if (fail) break;

                // We were able to fit another decomposed fraction into the original one.
                power++;

                // Try again on the reduced number.
                factorN = factorNbuffer;
                factorD = factorDbuffer;
                factorNbuffer = new List<BigInteger>();
                factorDbuffer = new List<BigInteger>();
            }

            // If increasing the power failed, return all buffered factors back to the decomposition
            while (factorNbuffer.Count > 0)
            {
                numerator.Add(factorNbuffer[0]);
                factorNbuffer.RemoveAt(0);
            }
            while (factorDbuffer.Count > 0)
            {
                denominator.Add(factorDbuffer[0]);
                factorDbuffer.RemoveAt(0);
            }

            // Compose the resulting number.
            BigInteger newNumerator = 1;
            BigInteger newDenominator = 1;
            foreach (BigInteger bi in numerator)
            {
                newNumerator *= bi;
            }
            foreach (BigInteger bi in denominator)
            {
                newDenominator *= bi;
            }

            return ((positive ? 1 : -1) * power, new OpRational((positive ? newNumerator : newDenominator), (positive ? newDenominator : newNumerator)));
        }
    }
}
