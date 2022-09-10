using System;
using System.Collections.Generic;
using System.Text;

namespace Stredoskolak
{
    public static class Settings
    {
        static Settings()
        {
            MaxDecompositionFactor = 101;
        }

        public static int MaxDecompositionFactor
        {
            get
            {
                return maxDecompositionFactor;
            }
            set
            {
                SetMaxFactor(value);
            }
        }
        public static int MaxPolynomialDegree { get; set; } = 2;
        public static int MaxExponentFactorOfRoot { get; set; } = 3;
        public static int MaxSizeOfNumbersInBytes { get; set; } = 15;

        public static int MaxGuessedNumbers { get; set; } = 7;
        private static int guessedNumberComplexity = 7;
        public static int GuessedNumberComplexity
        {
            get
            {
                return guessedNumberComplexity;
            }
            set
            {
                if (value < 2) guessedNumberComplexity = 2;
                else guessedNumberComplexity = value;
            }
        }

        public static int MaxResultingTermsFromExpansion { get; set; } = 24;
        public static int MaxTermsForTrigonometricSumExpansion { get; set; } = 4;

        public static int MaxBranchesExplored { get; set; } = 4096;
        public static int MaxSolutionsReturned { get; set; } = 8;

        public static int MaxDepthForSimplification { get; set; } = 30;

        public static bool CommentateCalculations { get; set; } = false;
        public static bool AproximatelyEvaluate { get; set; } = false;
        public static int MaxReturnedFails { get; set; } = 20;

        public static string OutputFormat = "default";

        public static IEnumerable<int> GetPrimes()
        {
            foreach (int p in primes)
            {
                if (p <= MaxDecompositionFactor) yield return p;
                else break;
            }
            yield break;
        }
        public static IEnumerable<int> GetPrimes(int maxFactor)
        {
            foreach (int p in primes)
            {
                if (p <= MaxDecompositionFactor && p <= maxFactor) yield return p;
                else break;
            }
            yield break;
        }

        public static IEnumerable<Expression> GetGuessedNumbers()
        {
            IEnumerator<Expression> generator = NumberEnumerators.RealExpressions.GetEnumerator();
            for (int i = 0; i < MaxGuessedNumbers; i++)
            {
                generator.MoveNext();
                yield return generator.Current;
            }
            yield break;
        }

        private static int maxDecompositionFactor = 3;
        private static void SetMaxFactor(int newValue)
        {
            if (newValue < 3) newValue = 3;
            for (int i = primes[primes.Count - 1] + 2; i <= newValue; i+=2)
            {
                bool isPrime = true;
                foreach (int prime in primes)
                {
                    if (i % prime == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime) primes.Add(i);
            }
            maxDecompositionFactor = newValue;
        }

        static readonly List<int> primes = new List<int>() { 2, 3 };
    }
}
