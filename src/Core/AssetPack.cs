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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gowtu
{
    public class AssetPack : IDisposable
    {
        private struct AssetData
        {
            public long dataLength;
            public long dataOffset;
            public ushort nameLength;
            public string name;         //Destination file path of the file in the pack
            public string filepath;     //Source file path of the initial file on disk

            public string GetFileName()
            {
                return Path.GetFileName(name);
            }

            public AssetData(string filepath, string filename, long dataLength, long dataOffset, ushort nameLength)
            {
                this.filepath = filepath;
                this.name = filename;
                this.dataLength = dataLength;
                this.dataOffset = dataOffset;
                this.nameLength = nameLength;
                this.name = filename;
            }
        }

        private List<AssetData> fileData;
        private List<AssetPackInfo> files;
        private string name;
        private string key;
        private bool loaded;

        public string Name
        {
            get
            {
                return name;
            }
        }

        public List<AssetPackInfo> Files
        {
            get
            {
                return files;
            }
        }

        public bool Loaded
        {
            get
            {
                return loaded;
            }
        }

        public AssetPack()
        {
            this.name = "";
            this.key = "";
            fileData = new List<AssetData>();
            files = new List<AssetPackInfo>();
        }

        /// <summary>
        /// Opens a ResourcePack from a filepath
        /// </summary>
        /// <param name="name">The filepath of the ResourcePack</param>
        /// <param name="key">The key/password of the ResourcePack</param>
        public AssetPack(string name, string key)
        {
            fileData = new List<AssetData>();
            files = new List<AssetPackInfo>();
            this.name = name;
            this.key = key;
            Load(name, key);
        }

        /// <summary>
        /// Opens a ResourcePack from an array of bytes
        /// </summary>
        /// <param name="name">The name of the ResourcePack (not a filepath!)</param>
        /// <param name="key">The key/password of the ResourcePack</param>
        /// <param name="data">The data of the ResourcePack</param>
        public AssetPack(string name, string key, byte[] data)
        {
            fileData = new List<AssetData>();
            files = new List<AssetPackInfo>();
            this.name = name;
            this.key = key;
            Load(name, key, data);
        }        

        public AssetPackStatus AddFile(string srcFilePath, string destFilePath = "")
        {
            if(!File.Exists(srcFilePath))  //File on disk does not exist
                return AssetPackStatus.FileNotFound;

            if(FileExists(srcFilePath) || FileExists(destFilePath))    //File already exists in this pack
                return AssetPackStatus.FileExists;

            //If no name is passed, just use the actual filepath as the name of the file in the pack
            if(destFilePath == "")
                destFilePath = srcFilePath;
            
            srcFilePath = srcFilePath.Replace('\\', '/');
            destFilePath = destFilePath.Replace('\\', '/');

            FileInfo file = new FileInfo(srcFilePath);
            fileData.Add(new AssetData(srcFilePath, destFilePath, file.Length, 0, 0));
            return AssetPackStatus.Ok;
        }

        public bool FileExists(string filename)
        {
            if(fileData.Count == 0)
                return false;

            for(int i = 0; i < fileData.Count; i++)
            {
                if(fileData[i].name == filename)
                {
                    return true;
                }
            }

            return false;
        }

        public AssetPackInfo GetFileInfo(string filename)
        {
            if(files.Count == 0)
                return null;

            for(int i = 0; i < files.Count; i++)
            {
                if(files[i].Name == filename)
                {
                    return files[i];
                }
            }

            return null;
        }

        public byte[] GetFileBuffer(AssetPackInfo info)
        {
            return GetFileBuffer(info.Name);
        }

        public byte[] GetFileBuffer(string filename)
        {
            if(!loaded)
                return null;

            var info = GetFileInfo(filename);

            if(info == null)
                return null;

            long length = info.Length;
            long offset = info.Offset;

            byte[] data = new byte[length];

            using(var fileStream = new FileStream(name, FileMode.Open))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);

                long totalBytes = length;
                int bufferOffset = 0;

                while(totalBytes > 0)
                {
                    int bytesRead = fileStream.Read(data, bufferOffset, data.Length);
                    totalBytes -= bytesRead;
                    bufferOffset += bytesRead;
                }
            }

            return data;
        }

        public async Task<byte[]> GetFileBufferAsync(string filename)
        {
            if(!loaded)
                return null;

            var info = GetFileInfo(filename);

            if(info == null)
                return null;

            long length = info.Length;
            long offset = info.Offset;

            byte[] data = new byte[length];

            using(var fileStream = new FileStream(name, FileMode.Open))
            {
                fileStream.Seek(offset, SeekOrigin.Begin);

                long totalBytes = length;
                int bufferOffset = 0;

                while(totalBytes > 0)
                {
                    int bytesRead = await fileStream.ReadAsync(data, bufferOffset, data.Length);
                    totalBytes -= bytesRead;
                    bufferOffset += bytesRead;
                }
            }

            return data;
        }

        public AssetPackFileStream GetFileStream(AssetPackInfo info)
        {
            return GetFileStream(info.Name);
        }

        public AssetPackFileStream GetFileStream(string filename)
        {
            if(!loaded)
                return null;

            var info = GetFileInfo(filename);

            if(info == null)
                return null;

            long offset = info.Offset;
            long length = info.Length;

            var stream = new AssetPackFileStream(name, offset, length);            
            return stream;
        }

        private Stream OpenStream(string filepath, byte[] data)
        {
            if(data == null)
            {
                return new FileStream(name, FileMode.Open);
            }
            else
            {
                return new MemoryStream(data);
            }
        }

        /// <summary>
        /// Opens a ResourcePack from a filepath or an array of bytes
        /// </summary>
        /// <param name="name">The filepath or name of the ResourcePack</param>
        /// <param name="key">The key/password of the ResourcePack</param>
        /// <param name="data">The data of the ResourcePack. If null then 'name' will be used as a filepath to load the ResourcePack</param>
        public AssetPackStatus Load(string name, string key, byte[] data = null)
        {
            if(!File.Exists(name))
            {
                return AssetPackStatus.FileNotFound;
            }

            if(loaded)
            {
                return AssetPackStatus.AlreadyLoaded;
            }

            this.name = name;
            
            fileData.Clear();
            files.Clear();

            byte[] buffer = new byte[sizeof(int)];

            using(var fileStream = OpenStream(name, data))
            {
                fileStream.Read(buffer, 0, sizeof(ushort));
                ushort checksumA = BinaryConverter.ToUInt16(buffer, 0, ByteOrder.BigEndian);

                fileStream.Read(buffer, 0, sizeof(int));
                int headerSize = BinaryConverter.ToInt32(buffer, 0, ByteOrder.BigEndian);

                byte[] header = new byte[headerSize];
                int totalBytes = headerSize;
                int readOffset = 0;

                while(totalBytes > 0)
                {
                    int bytesRead = fileStream.Read(header, readOffset, header.Length);
                    readOffset += bytesRead;
                    totalBytes -= bytesRead;                
                }

                Scramble(header, 0, headerSize, key);
                
                ushort checksumB = CalculateChecksum(header, headerSize);

                //If the passed key is correct, checksum A will match checksum B
                //If they do not match, the key is either wrong or the data in the header has been corrupted
                if(checksumA != checksumB)
                {
                    loaded = false;
                    return AssetPackStatus.ChecksumError;
                }

                int numFiles = BinaryConverter.ToInt32(header, 0, ByteOrder.BigEndian);
                readOffset = sizeof(int);

                for(int i = 0; i < numFiles; i++)
                {
                    long dataLength = BinaryConverter.ToInt64(header, readOffset, ByteOrder.BigEndian);                    
                    readOffset += sizeof(long);
                    long dataOffset = BinaryConverter.ToInt64(header, readOffset, ByteOrder.BigEndian);                    
                    readOffset += sizeof(long);
                    ushort nameLength = BinaryConverter.ToUInt16(header, readOffset, ByteOrder.BigEndian);
                    readOffset += sizeof(ushort);
                    string filename = BinaryConverter.ToString(header, readOffset, nameLength, TextEncoding.UTF8);
                    readOffset += nameLength;
                    fileData.Add(new AssetData(filename, filename, dataLength, dataOffset, nameLength));
                    files.Add(new AssetPackInfo(filename, dataLength, dataOffset));
                }
            }

            loaded = true;
            return AssetPackStatus.Ok;
        }

        public bool Save(string filepath, string key)
        {
            if(fileData.Count == 0)
                return false;

            if(loaded)
                Dispose();

            this.name = filepath;

            //ushort checksum 
            //int headerSize
            //int numFiles
            //for(numFiles)
            //  - long datalength
            //  - long dataOffset
            //  - ushort nameLength
            //  - string name
            //byte[] data ...

            using(var fileStream = new FileStream(filepath, FileMode.OpenOrCreate))
            {
                byte[] writeBuffer = new byte[4096];

                ushort checksum = 0;

                BinaryConverter.GetBytes(checksum, writeBuffer, 0, ByteOrder.BigEndian);
                fileStream.Write(writeBuffer, 0, sizeof(ushort));

                int headerSize = CalculateHeaderSize();
                BinaryConverter.GetBytes(headerSize, writeBuffer, 0, ByteOrder.BigEndian);
                fileStream.Write(writeBuffer, 0, sizeof(int));
                
                BinaryConverter.GetBytes(fileData.Count, writeBuffer, 0, ByteOrder.BigEndian);
                fileStream.Write(writeBuffer, 0, sizeof(int));

                for(int i = 0; i < fileData.Count; i++)
                {
                    var asset = fileData[i];
                    //Calculate on what offset the data will be placed
                    asset.dataOffset = CalculateFileOffset(headerSize, i);
                    
                    //Write data length
                    BinaryConverter.GetBytes(asset.dataLength, writeBuffer, 0, ByteOrder.BigEndian);
                    fileStream.Write(writeBuffer, 0, sizeof(long));

                    //Write data offset
                    BinaryConverter.GetBytes(asset.dataOffset, writeBuffer, 0, ByteOrder.BigEndian);
                    fileStream.Write(writeBuffer, 0, sizeof(long));

                    //Write length of filename in number of bytes
                    ushort nameLength = (ushort)BinaryConverter.GetBytes(asset.name, writeBuffer, 0, TextEncoding.UTF8);
                    asset.nameLength = nameLength;
                    BinaryConverter.GetBytes(nameLength, writeBuffer, 0, ByteOrder.BigEndian);
                    fileStream.Write(writeBuffer, 0, sizeof(ushort));

                    //Write filename
                    BinaryConverter.GetBytes(asset.name, writeBuffer, 0, TextEncoding.UTF8);
                    fileStream.Write(writeBuffer, 0, nameLength);

                    fileData[i] = asset;
                }

                for(int i = 0; i < fileData.Count; i++)
                {
                    using(var fileReader = new FileStream(fileData[i].filepath, FileMode.Open))
                    {
                        long totalBytes = fileData[i].dataLength;
                        
                        while(totalBytes > 0)
                        {
                            int bytesRead = fileReader.Read(writeBuffer, 0, writeBuffer.Length);
                            totalBytes -= bytesRead;
                            fileStream.Write(writeBuffer, 0, bytesRead);
                        }
                    }
                }

                int beginOfHeader = sizeof(ushort) + sizeof(int);

                fileStream.Seek(beginOfHeader, SeekOrigin.Begin);
                
                byte[] header = new byte[headerSize];

                long numBytes = headerSize;
                int offset = 0;
                
                while(numBytes > 0)
                {
                    int bytesRead = fileStream.Read(header, offset, header.Length);
                    numBytes -= bytesRead;
                    offset += bytesRead;
                }

                //Get a checksum of the header before scrambling
                //This is an extra security for when someone passes in a wrong key
                //In case of trying to load a pack with a wrong key, the calculated checksum will not match the original                
                checksum = CalculateChecksum(header, headerSize);
                BinaryConverter.GetBytes(checksum, writeBuffer, 0, ByteOrder.BigEndian);
                fileStream.Seek(0, SeekOrigin.Begin);
                fileStream.Write(writeBuffer, 0, sizeof(ushort));

                Scramble(header, 0, headerSize, key);

                fileStream.Seek(beginOfHeader, SeekOrigin.Begin);
                fileStream.Write(header, 0, header.Length);
            }

            loaded = true;
            return true;
        }

        private int CalculateHeaderSize()
        {
            byte[] writeBuffer = new byte[4096];
            
            int headerSize = 0;

            headerSize += sizeof(int); //4 bytes that describe the number of assets

            for(int i = 0; i < fileData.Count; i++)
            {
                headerSize += sizeof(long); //data length
                headerSize += sizeof(long); //data offset
                headerSize += sizeof(ushort); //name length
                headerSize += BinaryConverter.GetBytes(fileData[i].name, writeBuffer, 0, TextEncoding.UTF8); //name
            }

            return headerSize;
        }

        private long CalculateFileOffset(int headerSize, int fileIndex)
        {
            long offset = headerSize + 2 + 4;

            if(fileIndex == 0)
            {
                return offset;
            }
            
            for(int i = 1; i < fileData.Count; i++)
            {
                offset += fileData[i-1].dataLength;

                if(i == fileIndex)
                    break;
            }

            return offset;
        }

        private void Scramble(byte[] data, int index, int length, string key)
        {
            if(key == string.Empty)
                return;
            
            unsafe
            {
                fixed(byte* ptr = &data[index])
                {
                    int c = 0;
                    for(int i = 0; i < length; i++)
                    {
                        byte b = (byte)key[(c++) % key.Length];
                        int x = (int)ptr[i] ^ b;
                        ptr[i] = (byte)x;
                    }
                }
            }
        }

        private static unsafe UInt16 CalculateChecksum(byte[] buf, int length)
        {
            UInt32 sum = 0;

            fixed (byte* bufPointer = &buf[0])
            {
                byte* bufP = bufPointer;

                // build the sum of 16bit words
                while (length > 1)
                {
                    sum += (UInt32)(0xFFFF & (*bufP << 8 | *(bufP + 1)));
                    bufP += 2;
                    length -= 2;
                }

                // if there is a byte left then add it (padded with zero)
                if (length != 0)
                {
                    //--- made by SKA ---                sum += (0xFF & *buf)<<8;
                    sum += (UInt32)(0xFFFF & (*bufP << 8 | 0x00));
                }

                // now calculate the sum over the bytes in the sum
                // until the result is only 16bit long
                while ((sum >> 16) != 0)
                {
                    sum = (sum & 0xFFFF) + (sum >> 16);
                }
            }

            // build 1's complement:
            return ((UInt16)(sum ^ 0xFFFF));
        }

        public void Dispose()
        {
            loaded = false;
        }
    }

    public class AssetPackInfo
    {
        private string name;
        private long length;
        private long offset;

        public string Name => name;
        public long Length => length;
        public long Offset => offset;

        public AssetPackInfo(string name, long length, long offset)
        {
            this.name = name;
            this.length = length;
            this.offset = offset;
        }
    }

    public enum AssetPackStatus
    {
        AlreadyLoaded,
        ChecksumError,
        FileExists,
        FileNotFound,
        Ok
    }

    public class AssetPackFileStream : Stream, IDisposable
    {
        private FileStream stream;
        private string name;
        private long startOffset;
        private long endOffset;
        private long length;

        public override bool CanRead => stream.CanRead;

        public override bool CanSeek => stream.CanSeek;

        public override bool CanWrite => false;

        public override long Length => length;

        public override long Position 
        { 
            get
            {
                return stream.Position - startOffset;
            }
            set
            {
                long pos = startOffset + value;

                if(pos >= startOffset && pos < endOffset)
                {
                    stream.Position = pos;
                }
            } 
        }

        public AssetPackFileStream(string filepath, long startOffset, long length)
        {
            this.name = filepath;
            this.startOffset = startOffset;
            this.endOffset = startOffset + length;
            this.length = length;

            this.stream = new FileStream(filepath, FileMode.Open);
            this.stream.Seek(startOffset, SeekOrigin.Begin);
        }

        public override void Flush()
        {
            
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos = startOffset + offset;

            if(pos < startOffset || pos >= endOffset)
                return Position;            

            stream.Seek(pos, origin);
            
            return Position;
        }

        public override void SetLength(long value)
        {
            
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                stream?.Close();
                stream?.Dispose();
            }
        }
    }
}