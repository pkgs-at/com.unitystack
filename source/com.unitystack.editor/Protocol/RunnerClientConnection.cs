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
using System.IO;
using System.Net.Sockets;

namespace UnityStack.Editor.Protocol
{

    public class RunnerClientConnection : RunnerConnection
    {

        private int _processId;

        private RunnerCommand _command;

        private string _message;

        private int _exitCode;

        public RunnerClientConnection(TcpClient client)
            : base(client)
        {
            uint magic;

            magic = this.ReadUInt32();
            if (magic != MagicNumber)
                throw new IOException(String.Format(
                    "unexpected magic number: {0:X8}",
                    magic));
            this._processId = this.ReadInt32();
            this._command = RunnerCommand.None;
            this._message = null;
            this._exitCode = (-1);
        }

        public void Acknowledge()
        {
            this.WriteByte((byte)RunnerCommand.Acknowledge);
            this.Flush();
        }

        public bool Read()
        {
            this._command = (RunnerCommand)this.ReadByte();
            switch (this._command)
            {
                case RunnerCommand.Log:
                    this._message = this.ReadString();
                    return true;
                case RunnerCommand.Exit:
                    this._exitCode = (byte)this.ReadByte();
                    return false;
                default:
                    throw new InvalidOperationException();
            }
        }

        public RunnerCommand Command
        {
            get
            {
                return this._command;
            }
        }

        public int ProcessId
        {
            get
            {
                return this._processId;
            }
        }

        public string Message
        {
            get
            {
                return this._message;
            }
        }

        public int ExitCode
        {
            get
            {
                return this._exitCode;
            }
        }

    }

}
