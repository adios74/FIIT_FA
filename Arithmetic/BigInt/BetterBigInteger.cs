using System.Numerics;
using Arithmetic.BigInt.Interfaces;
using Arithmetic.BigInt.MultiplyStrategy;

namespace Arithmetic.BigInt;

public sealed class BetterBigInteger : IBigInteger
{
    private int _signBit;
    
    private uint _smallValue; // Если число маленькое, храним его прямо в этом поле, а _data == null.
    private uint[]? _data;
    
    public bool IsNegative => _signBit == 1;
    
    /// От массива цифр (little endian)
    public BetterBigInteger(uint[] digits, bool isNegative = false)
    {
        var iter = digits.Length - 1;
        while (iter >= 0 && digits[iter] == 0)
        {
            iter--;
        }

        if (iter == -1)
        {
            _smallValue = 0;
            _signBit = 0;
            _data = null;
            
        } else if (iter == 0)
        {
            _data = null;
            _smallValue = digits[0];
            if (isNegative)
            {
                _signBit = 1;
            }
            else
            {
                _signBit = 0;
            }
        }
        else
        {
            if (isNegative)
            {
                _signBit = 1;
            }
            else
            {
                _signBit = 0;
            }
            uint[] newDigits = new uint[iter + 1];
            int j = 0;
            while (j <= iter)
            {
                newDigits[j] = digits[j];
                j++;
            }
            _data = newDigits;
        }
    }
    
    public BetterBigInteger(IEnumerable<uint> digits, bool isNegative = false) :
        this(digits.ToArray(), isNegative)
    {
    }

    public BetterBigInteger(string value, int radix)
    {
        if (radix < 2 || radix > 36)
        {
            throw new ArgumentException("Некорректное значение системы счисления.");
        }

        bool isNegative = false;
        int start = 0;

        if (value.Length > 0)
        {
            if (value[0] == '-')
            {
                isNegative = true;
                start = 1;
            } else if (value[0] == '+')
            {
                start = 1;
            }
        }

        if (start == value.Length)
        {
            throw new ArgumentException("Некорректное значение числа.");
        }

        while (start < value.Length && value[start] == '0')
        {
            start++;
        }

        if (start == value.Length)
        {
            _signBit = 0;
            _smallValue = 0;
            _data = null;
            return;
        }

        var digits = new List<uint> { 0 };
        for (int i = start; i < value.Length; i++)
        {
            char c = value[i];
            int digit = GetDigitValue(c, radix);

            ulong carry = (ulong)digit;
            for (int j = 0; j < digits.Count; j++)
            {
                ulong mul = (ulong)digits[j] * (uint)radix + carry;
                digits[j] = (uint)mul;
                carry = mul >> 32;
            }

            while (carry != 0)
            {
                digits.Add((uint)carry);
                carry >>= 32;
            }
        }

        while (digits.Count > 0 && digits[digits.Count - 1] == 0)
        {
            digits.RemoveAt(digits.Count - 1);
        }

        if (digits.Count == 1 && digits[0] == 0)
        {
            _data = null;
            _smallValue = 0;
            _signBit = 0;
        } else if (digits.Count == 1)
        {
            if (isNegative)
            {
                _signBit = 1;
            }
            else
            {
                _signBit = 0;
            }
            _smallValue = digits[0];
            _data = null;
        }
        else
        {
            if (isNegative)
            {
                _signBit = 1;
            }
            else
            {
                _signBit = 0;
            }
            _data = digits.ToArray();
        }
    }

    private static int GetDigitValue(char c, int radix)
    {
        int value;
        if (c >= '0' && c <= '9')
        {
            value = c - '0';
        } else if (c >= 'A' && c <= 'Z')
        {
            value = c - 'A' + 10;
        } else if (c >= 'a' && c <= 'z')
        {
            value = c - 'a' + 10;
        }
        else
        {
            throw new ArgumentException("Невозможно перевести строку в число.");
        }

        if (value >= radix)
        {
            throw new ArgumentException("Некорректная система счисления.");
        }

        return value;
    }
    
    
    public ReadOnlySpan<uint> GetDigits()
    {
        return _data ?? [_smallValue];
    }


    public uint[] GetArrayOfDigits()
    {
        return _data ?? new uint[] { _smallValue };
    }

    private static bool IsZero(IBigInteger c)
    {
        var digits =  c.GetDigits();
        return digits.Length == 1 && digits[0] == 0;
    }

    public int CompareTo(IBigInteger? other)
    {
        if (other is null)
        {
            return 1;
        }

        if (ReferenceEquals(this, other))
        {
            return 0;
        }
        
        bool thisIsZero = IsZero(this);
        bool otherIsZero = IsZero(other);

        if (thisIsZero && otherIsZero)
        {
            return 0;
        }

        if (thisIsZero)
        {
            if (other.IsNegative)
            {
                return 1;
            }

            return -1;
        }

        if (otherIsZero)
        {
            if (this.IsNegative)
            {
                return -1;
            }

            return 1;
        }

        if (this.IsNegative != other.IsNegative)
        {
            if (this.IsNegative)
            {
                return -1;
            }

            return 1;
        }

        var thisDigits = GetDigits();
        var otherDigits = other.GetDigits();

        int comparison = CompareValue(thisDigits, otherDigits);
        if (this.IsNegative)
        {
            comparison = -comparison;
        }
        return comparison;
    }


    private static int CompareValue(ReadOnlySpan<uint> a, ReadOnlySpan<uint> b)
    {
        if (a.Length != b.Length)
        {
            return a.Length.CompareTo(b.Length);
        }

        for (int i = a.Length - 1; i >= 0; i--)
        {
            if (a[i] != b[i])
            {
                return a[i].CompareTo(b[i]);
            }
        }

        return 0;
    }


    public bool Equals(IBigInteger? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (this.IsNegative != other.IsNegative)
        {
            return false;
        }

        if (IsZero(this))
        {
            return IsZero(other);
        }

        if (this.CompareTo(other) == 0)
        {
            return true;
        }

        return false;
    }
    
    
    public override bool Equals(object? obj) => obj is IBigInteger other && Equals(other);

    public override int GetHashCode()
    {
        var hash =  new HashCode();
        hash.Add(IsNegative);
        foreach (var digit in GetDigits())
        {
            hash.Add(digit);
        }
        return hash.ToHashCode();
    }


    public static BetterBigInteger operator +(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(a))
        {
            return b;
        }
        
        if (IsZero(b))
        {
            return a;
        }

        if (a.IsNegative == b.IsNegative)
        {
            var sum = AddValues(a.GetArrayOfDigits(), b.GetArrayOfDigits());
            return new  BetterBigInteger(sum, a.IsNegative);
        }
        else
        {
            int comparison = CompareValue(a.GetArrayOfDigits(), b.GetArrayOfDigits());
            if (comparison >= 0)
            {
                var difference = SubtractValues(a.GetArrayOfDigits(), b.GetArrayOfDigits());
                return new   BetterBigInteger(difference, a.IsNegative);
            }
            else
            {
                var difference = SubtractValues(b.GetArrayOfDigits(), a.GetArrayOfDigits());
                return new   BetterBigInteger(difference, b.IsNegative);
            }
        }
    }

    private static uint[] AddValues(uint[] a, uint[] b)
    {
        int length = Math.Max(a.Length, b.Length);
        var result = new List<uint>(length + 1);
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
            result.Add((uint)sum);
            carry = sum >> 32;
        }

        if (carry != 0)
        {
            result.Add((uint)carry);
        }
        return result.ToArray();
    }


    private static uint[] SubtractValues(uint[] a, uint[] b)
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


    public static BetterBigInteger operator -(BetterBigInteger a, BetterBigInteger b)
        => a + (-b);

    
    public static BetterBigInteger operator -(BetterBigInteger a)
    {
        if (IsZero(a))
        {
            return a;
        }

        return new BetterBigInteger(a.GetArrayOfDigits(), !a.IsNegative);
    }


    public static BetterBigInteger Zero => new BetterBigInteger(new uint[] { 0 }, false);

    
    
    private static BigInteger ToSystemBigInteger(BetterBigInteger value)
    {
        var digits = value.GetDigits();
        byte[] bytes = new byte[digits.Length * 4];
        
        for (int i = 0; i < digits.Length; i++)
        {
            uint word = digits[i];
            bytes[i * 4] = (byte)word;
            bytes[i * 4 + 1] = (byte)(word >> 8);
            bytes[i * 4 + 2] = (byte)(word >> 16);
            bytes[i * 4 + 3] = (byte)(word >> 24);
        }
        var abs = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);

        if (value.IsNegative)
        {
            return -abs;
        }
        
        return abs;
    }

    private static BetterBigInteger FromSystemBigInteger(BigInteger value)
    {
        if (value == 0)
        {
            return Zero;
        }

        bool isNegative = value < 0;
        BigInteger absValue = isNegative ? -value : value;
        byte[] bytes = absValue.ToByteArray();
        int length = bytes.Length;
        
        if (length > 0 && bytes[length - 1] == 0)
        {
            length--;
        }

        var digits = new List<uint>();
        for (int i = 0; i < length; i += 4)
        {
            uint word = 0;
            for (int j = 0; j < 4 && i + j < length; j++)
            {
                word |= (uint)bytes[i + j] << (j * 8);
            }
            digits.Add(word);
        }

        while (digits.Count > 1 && digits[digits.Count - 1] == 0)
        {
            digits.RemoveAt(digits.Count - 1);
        }

        return new BetterBigInteger(digits, isNegative);
    }

    public static BetterBigInteger operator /(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(b))
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        if (IsZero(a))
        {
            return Zero;
        }
        
        BigInteger bia = ToSystemBigInteger(a);
        BigInteger bib =  ToSystemBigInteger(b);
        BigInteger result = bia / bib;
        return FromSystemBigInteger(result);
    }


    public static BetterBigInteger operator %(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(b))
        {
            throw new DivideByZeroException("Деление на ноль.");
        }

        if (IsZero(a))
        {
            return Zero;
        }
        
        BigInteger bia = ToSystemBigInteger(a);
        BigInteger bib =  ToSystemBigInteger(b);
        BigInteger result = bia % bib;
        return FromSystemBigInteger(result);
    }

    
    private static readonly IMultiplier Simple = new SimpleMultiplier();
    private static readonly IMultiplier Karatsuba = new KaratsubaMultiplier();
    private static readonly IMultiplier Fft = new FftMultiplier();

    public static BetterBigInteger operator *(BetterBigInteger a, BetterBigInteger b)
    {
        if (IsZero(a) || IsZero(b))
            return Zero;

        int max = Math.Max(a.GetDigits().Length, b.GetDigits().Length);
        
        IMultiplier multiplier = max switch
        {
            <= 64 => Simple,
            <= 256 => Karatsuba,
            >= 1024 => Fft,
            _ => Karatsuba
        };

        return multiplier.Multiply(a, b);
    }

    
    
    public static BetterBigInteger operator ~(BetterBigInteger a)
    {
        if (IsZero(a))
        {
            return new BetterBigInteger(new uint[] { 1 }, true);
        }

        var (words, sign) = ToTwosComplement(a);
        
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = ~words[i];
        }
    
        return FromTwosComplement(words);
    }
    
    

    private static (uint[] words, bool isNegative) ToTwosComplement(BetterBigInteger value)
    {
        if (!value.IsNegative)
        {
            uint[] abs = value.GetArrayOfDigits();
            
            if (abs.Length > 0 && (abs[^1] & 0x80000000) != 0)
            {
                Array.Resize(ref abs, abs.Length + 1);
                abs[^1] = 0;
            }
            
            return (abs, false);
        }

        uint[] digits = value.GetArrayOfDigits();
        uint[] inverted = new uint[digits.Length];
        
        for (int i = 0; i < digits.Length; i++)
        {
            inverted[i] = ~digits[i];
        }

        ulong carry = 1;
        
        for (int i = 0; i < inverted.Length; i++)
        {
            ulong sum = inverted[i] + carry;
            inverted[i] = (uint)sum;
            carry = sum >> 32;
            
            if (carry == 0)
            {
                break;
            }
        }

        if (carry != 0)
        {
            Array.Resize(ref inverted, inverted.Length + 1);
            inverted[^1] = (uint)carry;
        }

        if (inverted.Length > 0 && (inverted[^1] & 0x80000000) == 0)
        {
            Array.Resize(ref inverted, inverted.Length + 1);
            inverted[^1] = 0xFFFFFFFF;
        }

        return (inverted, true);
    }
    
    
    private static BetterBigInteger FromTwosComplement(uint[] twos)
    {
        if (twos.Length == 0)
        {
            return Zero;
        }

        bool isNegative = (twos[twos.Length - 1] & 0x80000000) != 0;
        
        if (!isNegative)
        {
            return new BetterBigInteger(twos, false);
        }
        
        var temporary = (uint[])twos.Clone();
        long borrow = 1;
        
        for (int i = 0; i < temporary.Length; i++)
        {
            long difference = temporary[i] - borrow;
            
            if (difference < 0)
            {
                difference += 0x100000000;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            
            temporary[i] = (uint)difference;
        }

        for (int i = 0; i < temporary.Length; i++)
        {
            temporary[i] = ~temporary[i];
        }
        
        int last = temporary.Length - 1;
        
        while (last >= 0 && temporary[last] == 0)
        {
            last--;
        }

        if (last < 0)
        {
            return Zero;
        }

        if (last < temporary.Length - 1)
        {
            Array.Resize(ref temporary, last + 1);
        }

        return new BetterBigInteger(temporary, true);
    }


    private static BetterBigInteger BitwiseOperation(BetterBigInteger a, BetterBigInteger b, char op)
    {
        var (awr, asg) = ToTwosComplement(a);
        var (bwr, bsg) = ToTwosComplement(b);
        int max = Math.Max(awr.Length, bwr.Length);
        var result = new uint[max];

        for (int i = 0; i < max; i++)
        {
            uint wa;
            if (i < awr.Length)
            {
                wa =  awr[i];
            }
            else
            {
                if (asg)
                {
                    wa = 0xFFFFFFFF;
                }
                else
                {
                    wa = 0;
                }
            }

            uint wb;
            if (i < bwr.Length)
            {
                wb =  bwr[i];
            }
            else
            {
                if (bsg)
                {
                    wb = 0xFFFFFFFF;
                }
                else
                {
                    wb = 0;
                }
            }

            result[i] = op switch
            {
                '&' => wa & wb,
                '|' => wa | wb,
                '^' => wa ^ wb,
                _ => throw new ArgumentException("Нет такой операции.")
            };
        }

        return FromTwosComplement(result);
    }


    public static BetterBigInteger operator &(BetterBigInteger a, BetterBigInteger b) 
        => BitwiseOperation(a, b, '&');
    public static BetterBigInteger operator |(BetterBigInteger a, BetterBigInteger b) 
        => BitwiseOperation(a, b, '|'); 
    public static BetterBigInteger operator ^(BetterBigInteger a, BetterBigInteger b) 
        => BitwiseOperation(a, b, '^');
    
    
    private static readonly BetterBigInteger MinusOne = new BetterBigInteger(new uint[] { 1 }, true);

    public static BetterBigInteger operator <<(BetterBigInteger a, int shift)
    {
        if (shift == 0)
        {
            return a;
        }

        if (IsZero(a))
        {
            return Zero;
        }

        if (shift < 0)
        {
            return a >> -shift;
        }

        if (a.IsNegative)
        {
            return -((-a) << shift);
        }

        var (words, _) = ToTwosComplement(a);

        int wordShift = shift / 32;
        int bitShift = shift % 32;

        int newLength = words.Length + wordShift + (bitShift > 0 ? 1 : 0);
        uint[] result = new uint[newLength];

        for (int i = 0; i < words.Length; i++)
        {
            ulong value = (ulong)words[i] << bitShift;
            int targetIndex = i + wordShift;

            result[targetIndex] |= (uint)value;

            if (bitShift > 0)
            {
                uint carry = (uint)(value >> 32);
                
                if (carry != 0)
                {
                    result[targetIndex + 1] |= carry;
                }
            }
        }

        return FromTwosComplement(result);
    }


    public static BetterBigInteger operator >>(BetterBigInteger a, int shift)
    {
        if (shift == 0)
        {
            return a;
        }

        if (IsZero(a))
        {
            return Zero;
        }

        if (shift < 0)
        {
            return a << -shift;
        }

        var (words, sign) = ToTwosComplement(a);
        int wordShift = shift / 32;
        int bitShift = shift % 32;

        if (wordShift >= words.Length)
        {
            if (sign)
            {
                return new BetterBigInteger(new uint[] { 1 }, true);
            }
            return Zero;
        }

        int length = words.Length - wordShift;
        var result = new uint[length];

        for (int i = wordShift; i < words.Length; i++)
        {
            ulong value = words[i];
            
            if (bitShift > 0)
            {
                value >>= bitShift;

                if (i + 1 < words.Length)
                {
                    ulong low = (ulong)words[i + 1] << (32 - bitShift);
                    value |= low;
                }
                else if (sign)
                {
                    value |= (ulong)0xFFFFFFFF << (32 - bitShift);
                }
            }

            result[i - wordShift] = (uint)value;
        }
        return FromTwosComplement(result);
    }
    
    public static bool operator ==(BetterBigInteger a, BetterBigInteger b) => Equals(a, b);
    public static bool operator !=(BetterBigInteger a, BetterBigInteger b) => !Equals(a, b);
    public static bool operator <(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) < 0;
    public static bool operator >(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) > 0;
    public static bool operator <=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) <= 0;
    public static bool operator >=(BetterBigInteger a, BetterBigInteger b) => a.CompareTo(b) >= 0;
    
    
    public override string ToString() => ToString(10);

    
    private static char FromDigitToChar(uint digit)
    {
        if (digit < 10)
        {
            return (char)('0' + digit);
        }
        return (char)('A' + digit - 10);
    }


    private static uint DivideByRadix(List<uint> digits, uint radix)
    {
        ulong remainder = 0;
        for (int i = digits.Count - 1; i >= 0; i--)
        {
            ulong value = (remainder << 32) | digits[i];
            digits[i] = (uint)(value / radix);
            remainder = value % radix;
        }

        while (digits.Count > 0 && digits[digits.Count - 1] == 0)
        {
            digits.RemoveAt(digits.Count - 1);
        }
        return (uint)remainder;
    }


    public string ToString(int radix)
    {
        if (radix < 2 || radix > 36)
        {
            throw new ArgumentException("Некорректная система счисления.");
        }

        if (IsZero(this))
        {
            return "0";
        }
        
        var digits = new List<uint>(GetDigits().ToArray());
        var chars = new List<char>();

        while (!(digits.Count == 1 && digits[0] == 0) && digits.Count != 0)
        {
            uint remainder = DivideByRadix(digits, (uint)radix);
            chars.Add(FromDigitToChar(remainder));
        }

        if (IsNegative)
        {
            chars.Add('-');
        }
        
        chars.Reverse();
        return new string(chars.ToArray());
    }
    
}