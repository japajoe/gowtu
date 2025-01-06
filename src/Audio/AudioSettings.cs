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
using System.IO;

namespace Gowtu
{
    public class AudioSettings
    {
        private static uint m_outputSampleRate = 44100;
        private static uint m_outputBufferSize = 4096;
        private static uint m_deviceId = 0;

        public static uint OutputSampleRate
        {
            get
            {
                return m_outputSampleRate;
            }
            set
            {
                m_outputSampleRate = value;
            }
        }

        public static uint OutputBufferSize
        {
            get
            {
                return m_outputBufferSize;
            }
            set
            {
                m_outputBufferSize = value;
            }
        }

        public static uint DeviceId
        {
            get
            {
                return m_deviceId;
            }
            set
            {
                m_deviceId = value;
            }
        }

        public static unsafe bool Load(string settingsPath)
        {
            if(!File.Exists(settingsPath))
            {
                Console.WriteLine("The audio settings file does not exist: " + settingsPath);
                return false;
            }

            //Data is stored in Big Endian (except for format identifier)
            //4 byte format identifier
            //4 byte version (uint)
            //4 byte checksum (uint)

            //4 byte outputSampleRate (int)
            //4 byte outputBufferSize (int)
            //4 byte deviceId (int)

            byte[] bytes = File.ReadAllBytes(settingsPath);

            if(bytes?.Length != 24)
            {
                Console.WriteLine("Invalid audio settings file");
                return false;
            }

            if(bytes[0] != 0xBA || bytes[1] != 0x55 || bytes[2] != 0xFA || bytes[3] != 0xDE)
            {
                Console.WriteLine("Invalid audio settings file");
                return false;
            }

            byte[] checksumBytes = new byte[20];
            Buffer.BlockCopy(bytes, 0, checksumBytes, 0, 4);
            Buffer.BlockCopy(bytes, 4, checksumBytes, 4, 4);
            Buffer.BlockCopy(bytes, 12, checksumBytes, 8, 4);
            Buffer.BlockCopy(bytes, 16, checksumBytes, 12, 4);
            Buffer.BlockCopy(bytes, 20, checksumBytes, 16, 4);

            uint version = ReadUInt32(bytes, 4);
            uint checksum = ReadUInt32(bytes, 8);
            uint sampleRate = ReadUInt32(bytes, 12);
            uint bufferSize = ReadUInt32(bytes, 16);
            uint deviceId = ReadUInt32(bytes, 20);

            Crc32 crc = new Crc32();

            if(crc.ComputeChecksum(checksumBytes) != checksum)
            {
                Console.WriteLine("Corrupt audio settings file");
                return false;
            }

            m_outputSampleRate = sampleRate;
            m_outputBufferSize = bufferSize;
            m_deviceId = deviceId;

            return true;
        }

        public static void Save(string settingsPath)
        {
            //Data is stored in Big Endian (except for format identifier)
            //4 byte format identifier
            //4 byte version (uint)
            //4 byte checksum (uint)

            //4 byte outputSampleRate (uint)
            //4 byte outputBufferSize (uint)
            //4 byte deviceId (uint)

            byte[] checksumBytes = new byte[20];

            checksumBytes[0] = 0xBA;
            checksumBytes[1] = 0x55;
            checksumBytes[2] = 0xFA;
            checksumBytes[3] = 0xDE;

            uint version = 1;

            WriteUInt32(version, checksumBytes, 4);
            WriteUInt32(m_outputSampleRate, checksumBytes, 8);
            WriteUInt32(m_outputBufferSize, checksumBytes, 12);
            WriteUInt32(m_deviceId, checksumBytes, 16);

            Crc32 crc = new Crc32();

            uint checksum = crc.ComputeChecksum(checksumBytes);

            byte[] outputBytes = new byte[24];
            Buffer.BlockCopy(checksumBytes, 0, outputBytes, 0, 8);

            WriteUInt32(checksum, outputBytes, 8);

            Buffer.BlockCopy(checksumBytes, 8, outputBytes, 12, 12);

            File.WriteAllBytes(settingsPath, outputBytes);

        }

        private static unsafe int ReadInt32(byte[] data, int offset)
        {
            fixed(byte *pBytes = &data[offset])
            {
                if(BitConverter.IsLittleEndian)
                    return (int)(pBytes[0] << 24 | pBytes[1] << 16 | pBytes[2] << 8 | pBytes[3]);
                else
                    return *(int*)pBytes;
            }
        }

        private static unsafe uint ReadUInt32(byte[] data, int offset)
        {
            fixed(byte *pBytes = &data[offset])
            {
                if(BitConverter.IsLittleEndian)
                    return (uint)(pBytes[0] << 24 | pBytes[1] << 16 | pBytes[2] << 8 | pBytes[3]);
                else
                    return *(uint*)pBytes;
            }
        }

        private static unsafe void WriteInt32(int value, byte[] data, int offset)
        {
            fixed(byte *pBytes = &data[offset])
            {
                if(BitConverter.IsLittleEndian)
                {
                    pBytes[0] = (byte)(value >> 24);
                    pBytes[1] = (byte)(value >> 16);
                    pBytes[2] = (byte)(value >> 8);
                    pBytes[3] = (byte)value;
                }
                else
                {
                    *(int*)pBytes = value;
                }
            }
        }

        private static unsafe void WriteUInt32(uint value, byte[] data, int offset)
        {
            fixed(byte *pBytes = &data[offset])
            {
                if(BitConverter.IsLittleEndian)
                {
                    pBytes[0] = (byte)(value >> 24);
                    pBytes[1] = (byte)(value >> 16);
                    pBytes[2] = (byte)(value >> 8);
                    pBytes[3] = (byte)value;
                }
                else
                {
                    *(uint*)pBytes = value;
                }
            }
        }
    }

    public class Crc32
    {
        private uint[] table;
        private const uint Polynomial = 0xedb88320;

        public Crc32()
        {
            // Initialize the CRC32 lookup table
            table = new uint[256];
            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (uint j = 8; j > 0; j--)
                {
                    if ((crc & 1) == 1)
                    {
                        crc = (crc >> 1) ^ Polynomial;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
                table[i] = crc;
            }
        }

        public uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff; // Initial value
            foreach (byte b in bytes)
            {
                byte tableIndex = (byte)((crc & 0xff) ^ b);
                crc = (crc >> 8) ^ table[tableIndex];
            }
            return ~crc; // Final XOR value
        }
    }
}