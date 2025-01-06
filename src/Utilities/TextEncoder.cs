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

using System.Text;

namespace Gowtu
{
    public enum TextEncoding
    {
        UTF8,
        UTF32,
        Unicode,
        ASCII
    }

    public sealed class TextEncoder
    {
        private UTF8Encoding utf8 = new UTF8Encoding();
        private UTF32Encoding utf32 = new UTF32Encoding();
        private UnicodeEncoding unicode = new UnicodeEncoding();
        private ASCIIEncoding ascii = new ASCIIEncoding();

        public TextEncoder()
        {
            utf8 = new UTF8Encoding();
            utf32 = new UTF32Encoding();
            unicode = new UnicodeEncoding();
            ascii = new ASCIIEncoding();
        }

        public int GetBytes(string value, int charIndex, int charCount, byte[] buffer, int offset, TextEncoding encoding)
        {
            int numBytes = 0;

            switch(encoding)
            {
                case TextEncoding.UTF8:
                    numBytes = utf8.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                case TextEncoding.UTF32:
                    numBytes = utf32.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                case TextEncoding.Unicode:
                    numBytes = unicode.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                case TextEncoding.ASCII:
                    numBytes = ascii.GetBytes(value, charIndex, charCount, buffer, offset);
                    break;
                default:
                    return 0;
            }

            return numBytes;  
        }

        public string ToString(byte[] buffer, int offset, int length, TextEncoding encoding)
        {
            switch(encoding)
            {
                case TextEncoding.UTF8:
                    return utf8.GetString(buffer, offset, length);
                case TextEncoding.UTF32:
                    return utf32.GetString(buffer, offset, length);
                case TextEncoding.Unicode:
                    return unicode.GetString(buffer, offset, length);
                case TextEncoding.ASCII:
                    return ascii.GetString(buffer, offset, length);
                default:
                    return string.Empty;
            }
        }

        public unsafe int GetByteCount(string value, int charIndex, int charCount, TextEncoding encoding)
        {
            int numBytes = 0;

            fixed(char *ptr = value)
            {
                char *chars = &ptr[charIndex];

                switch(encoding)
                {
                    case TextEncoding.UTF8:
                        numBytes = utf8.GetByteCount(chars, charCount);
                        break;
                    case TextEncoding.UTF32:
                        numBytes = utf32.GetByteCount(chars, charCount);
                        break;
                    case TextEncoding.Unicode:
                        numBytes = unicode.GetByteCount(chars, charCount);
                        break;
                    case TextEncoding.ASCII:
                        numBytes = ascii.GetByteCount(chars, charCount);
                        break;
                    default:
                        return 0;
                }
            }

            return numBytes;    
        }
    }
}