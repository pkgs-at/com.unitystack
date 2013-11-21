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
using System.Collections.Generic;
using System.Net.Sockets;
using At.Pkgs.Logging;
using UnityEditor;
using UnityStack.Logging;
using UnityStack.Editor.Protocol;
using UnityStack.Editor.Base;

namespace UnityStack.Editor
{

    public class Runner : UnityLogWriter
    {

        private RunnerServerConnection _connection;

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
                this._connection =
                    new RunnerServerConnection(new TcpClient(hostname, port));
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

        public void WriteLog(LogEntity entity, string message)
        {
            lock (this._connection)
            {
                this._connection.Log(message);
            }
        }

        protected void Exit(int code)
        {
            try
            {
                this._connection.Exit(code);
                this._connection.Close();
            }
            finally
            {
                EditorApplication.Exit(code);
            }
        }

        protected void ExecuteBatch()
        {
            int code;

            code = -1;
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
                        "invalid class for batch: {0}",
                        type.FullName));
                batch = (BaseBatch)Activator.CreateInstance(type);
                code = batch.Main(this.GetSubArguments(1));
            }
            catch (Exception throwable)
            {
                this._log.Error(throwable, "abort");
            }
            finally
            {
                this.Exit(code);
            }
        }

        public static void Batch()
        {
            new Runner().ExecuteBatch();
        }

    }

}
