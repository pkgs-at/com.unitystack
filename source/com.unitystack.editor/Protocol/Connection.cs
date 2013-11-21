/*
 * Copyright (c) 2009-2013, Architector Inc., Japan
 * All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace UnityStack.Editor.Protocol
{

    public abstract class Connection
    {

        private static readonly Encoding _encoding = new UTF8Encoding();

        private readonly TcpClient _client;

        private readonly NetworkStream _network;

        private readonly BufferedStream _stream;

        private readonly BinaryReader _reader;

        private readonly BinaryWriter _writer;

        public Connection(TcpClient client)
        {
            this._client = client;
            this._network = client.GetStream();
            this._stream = new BufferedStream(this._network);
            this._reader = new BinaryReader(this._stream);
            this._writer = new BinaryWriter(this._stream);
        }

        protected TcpClient TcpClient
        {
            get
            {
                return this._client;
            }
        }

        protected BinaryReader Reader
        {
            get
            {
                return this._reader;
            }
        }

        protected BinaryWriter Writer
        {
            get
            {
                return this._writer;
            }
        }

        protected sbyte ReadSByte()
        {
            return this._reader.ReadSByte();
        }

        protected void WriteSByte(sbyte value)
        {
            this._writer.Write(value);
        }

        protected byte ReadByte()
        {
            return this._reader.ReadByte();
        }

        protected void WriteByte(byte value)
        {
            this._writer.Write(value);
        }

        protected short ReadInt16()
        {
            return IPAddress.NetworkToHostOrder(this._reader.ReadInt16());
        }

        protected void WriteInt16(short value)
        {
            this._writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        protected ushort ReadUInt16()
        {
            return (ushort)this.ReadInt16();
        }

        protected void WriteUInt16(ushort value)
        {
            this.WriteInt16((short)value);
        }

        protected int ReadInt32()
        {
            return IPAddress.NetworkToHostOrder(this._reader.ReadInt32());
        }

        protected void WriteInt32(int value)
        {
            this._writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        protected uint ReadUInt32()
        {
            return (uint)this.ReadInt32();
        }

        protected void WriteUInt32(uint value)
        {
            this.WriteInt32((int)value);
        }

        protected long ReadInt64()
        {
            return IPAddress.NetworkToHostOrder(this._reader.ReadInt64());
        }

        protected void WriteInt64(long value)
        {
            this._writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        protected ulong ReadUInt64()
        {
            return (ulong)this.ReadInt64();
        }

        protected void WriteUInt64(ulong value)
        {
            this.WriteInt64((long)value);
        }

        protected byte[] ReadBytes(int length)
        {
            byte[] buffer;

            buffer = this._reader.ReadBytes(length);
            if (buffer.Length != length)
                throw new EndOfStreamException();
            return buffer;
        }

        protected void WriteBytes(byte[] buffer)
        {
            this._writer.Write(buffer);
        }

        protected string ReadString()
        {
            int length;
            byte[] buffer;

            length = this.ReadInt32();
            if (length < 0)
                return null;
            buffer = this.ReadBytes(length);
            return _encoding.GetString(buffer);
        }

        protected void WriteString(string value)
        {
            byte[] buffer;

            if (value == null)
            {
                this.WriteInt32(-1);
                return;
            }
            buffer = _encoding.GetBytes(value);
            this.WriteInt32(buffer.Length);
            this.WriteBytes(buffer);
        }

        public void Flush()
        {
            this._writer.Flush();
            this._stream.Flush();
            this._network.Flush();
        }

        public void Close()
        {
            this._writer.Close();
            this._reader.Close();
            this._stream.Close();
            this._network.Close();
            this._client.Close();
        }

    }

}
