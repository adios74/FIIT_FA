using System.Numerics;
using Arithmetic.BigInt.Interfaces;

namespace Arithmetic.BigInt.MultiplyStrategy;

internal class FftMultiplier : IMultiplier
{
    public BetterBigInteger Multiply(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(a) || IsZero(b))
        {
            return BetterBigInteger.Zero;
        }

        bool resultSign = a.IsNegative ^ b.IsNegative;
        var aDigits = a.GetDigits();
        var bDigits = b.GetDigits();

        int aLen16 = aDigits.Length * 2;
        int bLen16 = bDigits.Length * 2;
        int convLen = aLen16 + bLen16 - 1;
        int size = 1;
        
        while (size < convLen)
        {
            size <<= 1;
        }

        Complex[] A = new Complex[size];
        Complex[] B = new Complex[size];

        for (int i = 0; i < aDigits.Length; i++)
        {
            uint d = aDigits[i];
            A[i * 2] = new Complex(d & 0xFFFF, 0);
            A[i * 2 + 1] = new Complex(d >> 16, 0);
        }
        
        for (int i = 0; i < bDigits.Length; i++)
        {
            uint d = bDigits[i];
            B[i * 2] = new Complex(d & 0xFFFF, 0);
            B[i * 2 + 1] = new Complex(d >> 16, 0);
        }

        Fft(A, false);
        Fft(B, false);
        
        for (int i = 0; i < size; i++)
        {
            A[i] *= B[i];
        }
        
        Fft(A, true);

        long carry = 0;
        var digits16 = new List<ushort>();
        for (int i = 0; i < convLen; i++)
        {
            long val = (long)Math.Round(A[i].Real) + carry;
            digits16.Add((ushort)(val & 0xFFFF));
            carry = val >> 16;
        }
        
        while (carry > 0)
        {
            digits16.Add((ushort)(carry & 0xFFFF));
            carry >>= 16;
        }

        var digits32 = new List<uint>();
        for (int i = 0; i < digits16.Count; i += 2)
        {
            uint low = digits16[i];
            uint high = (i + 1 < digits16.Count) ? digits16[i + 1] : 0u;
            digits32.Add(low | (high << 16));
        }

        while (digits32.Count > 1 && digits32[^1] == 0)
        {
            digits32.RemoveAt(digits32.Count - 1);
        }

        return new BetterBigInteger(digits32, resultSign);
    }

    private static void Fft(Complex[] data, bool inverse)
    {
        int n = data.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; j >= bit; bit >>= 1)
            {
                j -= bit;
            }
            j += bit;
            
            if (i < j)
            {
                (data[i], data[j]) = (data[j], data[i]);
            }
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double angle = 2 * Math.PI / len * (inverse ? -1 : 1);
            Complex wlen = new Complex(Math.Cos(angle), Math.Sin(angle));
            
            for (int i = 0; i < n; i += len)
            {
                Complex w = Complex.One;
                for (int j = 0; j < len / 2; j++)
                {
                    Complex u = data[i + j];
                    Complex v = data[i + j + len / 2] * w;
                    data[i + j] = u + v;
                    data[i + j + len / 2] = u - v;
                    w *= wlen;
                }
            }
        }

        if (inverse)
        {
            for (int i = 0; i < n; i++)
            {
                data[i] /= n;
            }
        }
    }

    private static bool IsZero(BetterBigInteger x)
    {
        var digits = x.GetDigits();
        return digits.Length == 1 && digits[0] == 0;
    }
}