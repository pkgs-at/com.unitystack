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
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using At.Pkgs.Util;
using UnityStack.Editor.Protocol;
using UnityStack.Editor.Base;

namespace UnityStack.Editor.Runner
{

    public class RunnerClient
    {

        protected class ArgumentBuilder
        {

            private StringBuilder _builder;

            public ArgumentBuilder()
            {
                this._builder = new StringBuilder();
            }

            public void AppendUnityArguments(params string[] values)
            {
                foreach (string value in values)
                {
                    this._builder.Append(
                        Strings.QuoteCommandLineArgument(value));
                    this._builder.Append(' ');
                }
            }

            public void AppendRunnerArguments(params string[] values)
            {
                foreach (string value in values)
                {
                    this._builder.Append(
                        Strings.QuoteCommandLineArgument("!" + value));
                    this._builder.Append(' ');
                }
            }

            public override string ToString()
            {
                return this._builder.ToString();
            }

        }

        private string _unityPath;

        private string _projectPath;

        private string _logFile;

        public string UnityPath
        {
            get
            {
                return this._unityPath;
            }
            set
            {
                this._unityPath = value;
            }
        }

        public string UnityEditorFile
        {
            get
            {
                if (this._unityPath == null)
                    throw new NullReferenceException();
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.Win32NT:
                        return Path.Combine(
                            this._unityPath,
                            "Editor/Unity.exe");
                    case PlatformID.MacOSX:
                        return Path.Combine(
                            this._unityPath,
                            "Contents/MacOS/Unity");
                }
                throw new PlatformNotSupportedException();
            }
        }

        public string ProjectPath
        {
            get
            {
                return this._projectPath;
            }
            set
            {
                this._projectPath = value;
            }
        }

        public string LogFile
        {
            get
            {
                return this._logFile;
            }
            set
            {
                this._logFile = value;
            }
        }

        protected ArgumentBuilder ToArgumentBuilder()
        {
            ArgumentBuilder builder;

            builder = new ArgumentBuilder();
            builder.AppendUnityArguments(
                "-batchmode",
                "-quit");
            if (this._logFile != null)
                builder.AppendUnityArguments(
                    "-logFile",
                    this._logFile);
            if (this._projectPath == null)
                throw new NullReferenceException();
            builder.AppendUnityArguments(
                "-projectPath",
                this._projectPath);
            builder.AppendUnityArguments(
                "-executeMethod",
                "UnityStack.Editor.Runner.Batch");
            return builder;
        }

        protected ProcessStartInfo BuildStarter(
            IPEndPoint endPoint,
            string path,
            string batch,
            BaseBatchParameter parameter)
        {
            ProcessStartInfo starter;
            ArgumentBuilder arguments;

            starter = new ProcessStartInfo(this.UnityEditorFile);
            starter.CreateNoWindow = false;
            starter.UseShellExecute = false;
            starter.WindowStyle = ProcessWindowStyle.Hidden;
            arguments = this.ToArgumentBuilder();
            arguments.AppendRunnerArguments(
                endPoint.Address.ToString(),
                endPoint.Port.ToString());
            arguments.AppendRunnerArguments(
                path,
                batch);
            arguments.AppendRunnerArguments(parameter.ToArguments());
            starter.Arguments = arguments.ToString();
            return starter;
        }

        public delegate void Worker(RunnerClientConnection connection);

        public int Connect(
            string path,
            string batch,
            BaseBatchParameter parameter,
            Worker worker)
        {
            TcpListener listener;
            bool stopped;

            listener = null;
            stopped = false;
            try
            {
                Exception cause;
                int code;
                bool completed;
                Thread thread;
                Process launcher;

                cause = null;
                code = -1;
                completed = false;
                thread = new Thread(delegate()
                {
                    RunnerClientConnection connection;

                    connection = null;
                    try
                    {
                        TcpClient client;
                        Process process;

                        client = listener.AcceptTcpClient();
                        connection = new RunnerClientConnection(client);
                        process = Process.GetProcessById(connection.ProcessId);
                        connection.Acknowledge();
                        while (connection.Read()) worker(connection);
                        code = connection.ExitCode;
                        process.WaitForExit();
                        completed = true;
                    }
                    catch (Exception throwable)
                    {
                        cause = throwable;
                    }
                    finally
                    {
                        if(connection != null)
                            connection.Close();
                    }
                });
                listener = new TcpListener(new IPEndPoint(IPAddress.Loopback, 0));
                listener.Start();
                thread.Start();
                launcher = Process.Start(this.BuildStarter(
                    (IPEndPoint)listener.LocalEndpoint, path, batch, parameter));
                launcher.WaitForExit();
                if (launcher.ExitCode != 0)
                {
                    listener.Stop();
                    stopped = true;
                }
                thread.Join();
                if (!stopped && cause != null)
                    throw new Exception("failed on worker thread", cause);
                return completed ? code : launcher.ExitCode;
            }
            finally
            {
                if (listener != null && !stopped)
                    listener.Stop();
            }
        }

    }

}
