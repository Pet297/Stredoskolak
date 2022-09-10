using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Stredoskolak
{
    public static class Complexity
    {
        public static bool ShouldPowerBeCalculated(BigInteger powerBase ,BigInteger powerExponent)
        {
            int baseSize = powerBase.GetByteCount();
            BigInteger newCount = baseSize * powerExponent;

            if (newCount > Settings.MaxSizeOfNumbersInBytes) return false;
            else return true;
        }

        public static bool ShouldMultiplicationBeCalculated(BigInteger first, BigInteger second)
        {
            int size1 = first.GetByteCount();
            int size2 = second.GetByteCount();
            if (size1 + size2 > Settings.MaxSizeOfNumbersInBytes + 1) return false;
            return true;
        }
    }
}
