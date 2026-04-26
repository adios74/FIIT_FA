using System;
using System.Buffers;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class KaratsubaMultiplier : IMultiplier
{
    private readonly SimpleMultiplier _simple = new();

    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(a) || IsZero(b))
        {
            return BetterBigInteger.Zero;
        }

        bool resultIsNegative = a.IsNegative ^ b.IsNegative;

        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();

        uint[] product = KaratsubaMultiply(aDigits, bDigits);
        return new BetterBigInteger(product, resultIsNegative);
    }

    private static bool IsZero(BetterBigInteger value)
    {
        var digits = value.GetDigits();
        
        return digits.Length == 1 && digits[0] == 0;
    }

    private uint[] KaratsubaMultiply(ReadOnlySpan<uint> x, ReadOnlySpan<uint> y)
    {
        x = DeleteZeros(x);
        y = DeleteZeros(y);

        int n = Math.Max(x.Length, y.Length);

        if (n <= 32)
        {
            return SimpleMultiplier.MultiplyArrays(x.ToArray(), y.ToArray());
        }

        int m = (n + 1) / 2;

        ReadOnlySpan<uint> a0 = x.Slice(0, Math.Min(m, x.Length));
        ReadOnlySpan<uint> a1 = x.Length > m ? x.Slice(m) : default;
        ReadOnlySpan<uint> b0 = y.Slice(0, Math.Min(m, y.Length));
        ReadOnlySpan<uint> b1 = y.Length > m ? y.Slice(m) : default;

        uint[] sumX = Add(a0, a1);
        uint[] sumY = Add(b0, b1);

        uint[] part0 = KaratsubaMultiply(a0, b0);
        uint[] part2 = KaratsubaMultiply(a1, b1);
        uint[] part1 = KaratsubaMultiply(sumX, sumY);

        part1 = Subtract(part1, part0);
        part1 = Subtract(part1, part2);

        return Combine(part0, part1, part2, m);
    }

    private static uint[] Add(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        int length = Math.Max(a.Length, b.Length);
        var result = new uint[length + 1];
        ulong carry = 0;
        
        for (int i = 0; i < length; i++)
        {
            ulong sum = carry;
            
            if (i < a.Length)
            {
                sum += a[i];
            }

            if (i < b.Length)
            {
                sum += b[i];
            }
            
            result[i] = (uint)sum;
            carry = sum >> 32;
        }

        if (carry != 0)
        {
            result[length] = (uint)carry;
        }
        else
        {
            Array.Resize(ref result, length); 
        }
            
        return result;
    }

    private static uint[] Subtract(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        var result = new uint[a.Length];
        long borrow = 0;
        
        for (int i = 0; i < a.Length; i++)
        {
            long difference = a[i] - borrow;
            
            if (i < b.Length)
            {
                difference -= b[i];
            }
            
            if (difference < 0)
            {
                difference += 0x100000000;
                borrow = 1;
            }
            else
            {
                borrow = 0; 
            }
            
            result[i] = (uint)difference;
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

    private static uint[] Combine(uint[] part0, uint[] part1, uint[] part2, int m)
    {
        int totalLen = Math.Max(part0.Length, Math.Max(part1.Length + m, part2.Length + 2 * m));
        var result = new uint[totalLen];

        Array.Copy(part0, 0, result, 0, part0.Length);

        AddShifted(result, part1, m);

        AddShifted(result, part2, 2 * m);

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

    private static void AddShifted(uint[] dest, uint[] source, int shift)
    {
        ulong carry = 0;
        
        for (int i = 0; i < source.Length; i++)
        {
            int idx = i + shift;
            ulong sum = dest[idx] + (ulong)source[i] + carry;
            dest[idx] = (uint)sum;
            carry = sum >> 32;
        }
        
        int k = source.Length + shift;
        
        while (carry != 0 && k < dest.Length)
        {
            ulong sum = dest[k] + carry;
            dest[k] = (uint)sum;
            carry = sum >> 32;
            k++;
        }
    }

    private static ReadOnlySpan<uint> DeleteZeros(ReadOnlySpan<uint> span)
    {
        int last = span.Length - 1;
        
        while (last >= 0 && span[last] == 0)
        {
            last--;
        }

        if (last < 0)
        {
            return default;
        }

        return span.Slice(0, last + 1);
    }
}