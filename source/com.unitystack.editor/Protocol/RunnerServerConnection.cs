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
using System.Net.Sockets;

namespace UnityStack.Editor.Protocol
{

    public class RunnerServerConnection : RunnerConnection
    {

        public RunnerServerConnection(TcpClient client)
            : base(client)
        {
            this.WriteUInt32(MagicNumber);
            this.WriteInt32(Process.GetCurrentProcess().Id);
            this.Flush();
            if (this.ReadCommand() != RunnerCommand.Acknowledge)
                throw new InvalidOperationException();
        }

        protected RunnerCommand ReadCommand()
        {
            return (RunnerCommand)this.ReadByte();
        }

        public void Log(string message)
        {
            this.WriteByte((byte)RunnerCommand.Log);
            this.WriteString(message);
            this.Flush();
        }

        public void Exit(int code)
        {
            this.WriteByte((byte)RunnerCommand.Exit);
            this.WriteByte((byte)code);
            this.Flush();
        }

    }

}
