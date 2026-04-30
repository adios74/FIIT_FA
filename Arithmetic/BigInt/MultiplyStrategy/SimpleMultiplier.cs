using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class SimpleMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(a) || IsZero(b))
        {
            return BetterBigInteger.Zero;
        }
        
        bool productIsNegative = a.IsNegative ^  b.IsNegative;
        uint[] aDigits = a.GetArrayOfDigits();
        uint[] bDigits = b.GetArrayOfDigits();
        uint[] product = MultiplyValue(aDigits, bDigits);
        return new BetterBigInteger(product, productIsNegative);
    }
    
    
    public static uint[] MultiplyArrays(uint[] a, uint[] b)
    {
        if (a.Length == 1 && a[0] == 0 || b.Length == 1 && b[0] == 0)
        {
            return new uint[] { 0 };
        }
            
        return MultiplyValue(a, b);
    }


    private static bool IsZero(BetterBigInteger value)
    {
        var digits = value.GetDigits();
        if (digits.Length == 1 && digits[0] == 0)
        {
            return true;
        }

        return false;
    }


    private static uint[] MultiplyValue(uint[] a, uint[] b)
    {
        uint[] result = new uint[a.Length + b.Length];

        for (int i = 0; i < a.Length; i++)
        {
            ulong carry = 0;
            for (int j = 0; j < b.Length; j++)
            {
                int index = i + j;
                ulong product = (ulong)a[i] * b[j] + result[index] + carry;
                result[index] = (uint)product;
                carry = product >> 32;
            }

            int k = i + b.Length;
            while (carry != 0)
            {
                ulong sum = result[k] + carry;
                result[k] = (uint)sum;
                carry = sum >> 32;
                k++;
            }
        }

        int last = result.Length - 1;
        while (last >= 0 && result[last] == 0)
        {
            last--;
        }

        if (last < 0)
        {
            return new uint[] { 0 };
        }

        if (last < result.Length - 1)
        {
            Array.Resize(ref result, last + 1);
        }
        return result;
    }
}