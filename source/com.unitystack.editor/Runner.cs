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
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using At.Pkgs.Logging;
using UnityEditor;
using UnityStack;
using UnityStack.Logging;
using UnityStack.Editor.Base;

namespace UnityStack.Editor
{

    public class Runner : UnityLogWriter
    {

        private static readonly Encoding _encoding = new UTF8Encoding();

        private TcpClient _client;

        private BinaryWriter _writer;

        private string[] _arguments;

        private Log _log;

        protected Runner()
        {
            int index;
            string hostname;
            int port;
            string path;

            {
                string[] arguments;
                List<string> list;

                arguments = Environment.GetCommandLineArgs();
                list = new List<string>();
                foreach (string argument in arguments)
                {
                    if (argument.StartsWith("!"))
                        list.Add(argument.Substring(1));
                }
                index = 0;
                hostname = list[index++];
                port = UInt16.Parse(list[index++]);
                path = list[index++];
                list.RemoveRange(0, index);
                this._arguments = list.ToArray();
            }
            {
                Stream stream;

                this._client = new TcpClient(
                    hostname,
                    port);
                stream = new BufferedStream(this._client.GetStream());
                this._writer = new BinaryWriter(stream);
                this.WriteHead();
            }
            {
                LogManager manager;
                UnityDebugAppender appender;

                AbstractBootstrap.Path = path;
                manager = AbstractBootstrap.Instance.LogManager;
                appender = (UnityDebugAppender)
                    manager.Appender.Unwrap(typeof(UnityDebugAppender));
                if (appender != null) appender.Writer = this;
                this._log = manager.LogFor(this);
                this._log.Notice("initialized");
            }
        }

        protected string[] Arguments
        {
            get
            {
                return this._arguments;
            }
        }

        protected string[] GetSubArguments(int skip)
        {
            string[] arguments;

            arguments = new string[this._arguments.Length - skip];
            Array.Copy(this._arguments, skip, arguments, 0, arguments.Length);
            return arguments;
        }

        private void WriteByte(byte value)
        {
            this._writer.Write(value);
        }

        private void WriteSByte(sbyte value)
        {
            this.WriteByte((byte)value);
        }

        private void WriteBytes(byte[] values)
        {
            this._writer.Write(values);
        }

        private void WriteInt16(short value)
        {
            this._writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        private void WriteUInt16(ushort value)
        {
            this.WriteInt16((short)value);
        }

        private void WriteInt32(int value)
        {
            this._writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        private void WriteUInt32(uint value)
        {
            this.WriteInt32((int)value);
        }

        private void WriteInt64(long value)
        {
            this._writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        private void WriteUInt64(ulong value)
        {
            this.WriteInt64((long)value);
        }

        private void WriteString(string value)
        {
            byte[] bytes;
            long length;

            bytes = _encoding.GetBytes(value);
            length = bytes.LongLength;
            if (length > UInt32.MaxValue)
                throw new ArgumentOutOfRangeException();
            this.WriteUInt32((uint)bytes.LongLength);
            this.WriteBytes(bytes);
        }

        private void Flush()
        {
            this._writer.Flush();
        }

        private void Close()
        {
            this._writer.Close();
            this._client.Close();
        }

        private void WriteHead()
        {
            lock (this._writer)
            {
                this.WriteUInt32(0x1981ACE2);
                this.WriteInt32(Process.GetCurrentProcess().Id);
            }
        }

        public void WriteLog(LogEntity entity, string message)
        {
            lock (this._writer)
            {
                this.WriteByte(0x01);
                this.WriteString(message);
                this.Flush();
            }
        }

        private void WriteTail(int code)
        {
            lock (this._writer)
            {
                this.WriteByte(0xFE);
                this.WriteByte((byte)code);
                this.Flush();
            }
        }

        protected virtual void Exit(int code)
        {
            try
            {
                this._log.Notice(
                    "terminating with code: {0}",
                    code);
                this.WriteTail(code);
                this.Close();
            }
            finally
            {
                UnityEditor.EditorApplication.Exit(code);
            }
        }

        protected void ExecuteBatch()
        {
            try
            {
                string name;
                Type type;
                BaseBatch batch;

                name = this.Arguments[0];
                this._log.Notice("lookup batch: {0}", name);
                type = Type.GetType(name);
                if (type == null)
                    type = AbstractBootstrap.Instance.GetType(name);
                if (!typeof(BaseBatch).IsAssignableFrom(type))
                    throw new ArgumentException(String.Format(
                        "invalid class for Batch: {0}",
                        type.FullName));
                batch = (BaseBatch)Activator.CreateInstance(type);
                this.Exit(batch.Main(this.GetSubArguments(1)));
            }
            catch (Exception throwable)
            {
                this._log.Error(throwable, "abort");
                this.Exit(-1);
            }
        }

        public static void Batch()
        {
            new Runner().ExecuteBatch();
        }

    }

}
