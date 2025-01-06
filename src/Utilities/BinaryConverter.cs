// MIT License

// Copyright (c) 2025 W.M.R Jap-A-Joe

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;

namespace Gowtu
{
    public enum ByteOrder
    {
        LittleEndian,
        BigEndian
    }

    public static unsafe class BinaryConverter
    {
        private static TextEncoder encoder = new TextEncoder();

        private static void memcpy(void* dst, void* src, ulong n)
        {
            Buffer.MemoryCopy(src, dst, n, n);
        }

        private static void MemCopy(byte[] destination, int destinationOffset, byte* source, int sourceOffset, uint length)
        {
            fixed(byte* dest = &destination[destinationOffset])
            {
                memcpy(dest, &source[sourceOffset], (ulong)length);
            }
        }

        public static void GetBytes(Int64 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(Int64), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(Int64));
        }

        public static void GetBytes(UInt64 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(UInt64), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(UInt64));
        }

        public static void GetBytes(Int32 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(Int32), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(Int32));
        }        

        public static void GetBytes(UInt32 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(UInt32), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(UInt32));
        }

        public static void GetBytes(Int16 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(Int16), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(Int16));
        }

        public static void GetBytes(UInt16 value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(UInt16), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(UInt16));
        }

        public static void GetBytes(double value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(double), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(double));
        }

        public static void GetBytes(float value, byte[] buffer, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            byte* p = (byte*)&value;
            ConvertToByteOrder(p, sizeof(float), byteOrder);
            MemCopy(buffer, offset, p, 0, sizeof(float));
        }

        public static int GetBytes(string value, int charIndex, int charCount, byte[] buffer, int offset, TextEncoding encoding)
        {
            return encoder.GetBytes(value, charIndex, charCount, buffer, offset, encoding);
        }

        public static int GetBytes(string value, int charCount, byte[] buffer, int offset, TextEncoding encoding)
        {
            return GetBytes(value, 0, charCount, buffer, offset, encoding);
        }

        public static int GetBytes(string value, byte[] buffer, int offset, TextEncoding encoding)
        {
            return GetBytes(value, 0, value.Length, buffer, offset, encoding);
        }

        public static Int64 ToInt64(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(Int64), byteOrder);
                return *(Int64*)value;
            }
        }

        public static UInt64 ToUInt64(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(UInt64), byteOrder);
                return *(UInt64*)value;
            }
        }

        public static Int32 ToInt32(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(Int32), byteOrder);
                return *(Int32*)value;
            }
        }

        public static UInt32 ToUInt32(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(UInt32), byteOrder);
                return *(UInt32*)value;
            }
        }

        public static Int16 ToInt16(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(Int16), byteOrder);
                return *(Int16*)value;
            }
        }

        public static UInt16 ToUInt16(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(UInt16), byteOrder);
                return *(UInt16*)value;
            }
        }

        public static float ToSingle(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(float), byteOrder);
                return *(float*)value;
            }
        }

        public static double ToDouble(byte[] bytes, int offset, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            fixed(byte* value = &bytes[offset])
            {
                ConvertToByteOrder(value, sizeof(double), byteOrder);
                return *(double*)value;
            }
        }

        public static string ToString(byte[] buffer, int offset, int length, TextEncoding encoding)
        {
            return encoder.ToString(buffer, offset, length, encoding);
        }

        public static void FlipBytes(byte[] bytes, int offset, int length)
        {
            fixed(byte* ptr = &bytes[offset])
            {
                FlipBytes(ptr, length);
            }
        }

        private static void FlipBytes(byte* bytes, int length)
        {
            for (int i = 0; i < length / 2; ++i) 
            {
                byte t = bytes[i];
                bytes[i] = bytes[length - i - 1];
                bytes[length - i - 1] = t;
            }
        }

        private static void ConvertToByteOrder(byte *bytes, int length, ByteOrder byteOrder)
        {
            if(byteOrder == ByteOrder.LittleEndian)
            {
                if(!BitConverter.IsLittleEndian)
                    FlipBytes(bytes, length);
            }
            else
            {
                if(BitConverter.IsLittleEndian)
                    FlipBytes(bytes, length);
            }
        }

        public static int GetByteCount(string value, TextEncoding encoding)
        {
            return GetByteCount(value, 0, value.Length, encoding);
        }

        public static int GetByteCount(string value, int charCount, TextEncoding encoding)
        {
            return GetByteCount(value, 0, charCount, encoding);
        }

        public static int GetByteCount(string value, int charIndex, int charCount, TextEncoding encoding)
        {
            return encoder.GetByteCount(value, charIndex, charCount, encoding);
        }
    }
}