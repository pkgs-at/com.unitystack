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
using UnityStack.Editor.Base;
using UnityStack.Editor.Protocol;

namespace UnityStack.Editor.Runner
{

    public class Program
    {

        private readonly RunnerClient _client;

        protected Program(
            string unityPath,
            string projectPath,
            string logFile)
        {
            this._client = new RunnerClient();
            this._client.UnityPath = unityPath;
            this._client.ProjectPath = projectPath;
            this._client.LogFile = logFile;
        }

        protected int Execute(
            string path,
            string batch,
            BaseBatchParameter parameter)
        {
            return this._client.Connect(path, batch, parameter, delegate(RunnerClientConnection connection)
            {
                switch (connection.Command)
                {
                    case RunnerCommand.None:
                        break;
                    case RunnerCommand.Log:
                        Console.Out.WriteLine(connection.Message);
                        break;
                    default:
                        throw new InvalidOperationException(String.Format(
                            "unknown command: {0}",
                            connection.Command));
                }
            });
        }

        public static void Main(string[] arguments)
        {
            Program program;
            string[] parameters;

            if (arguments.Length < 4)
            {
                Console.Error.WriteLine(
                    "parameter:" +
                    " UnityPath ProjectPath LogFile BatchClass" +
                    " [BatchParameters...]");
                Environment.ExitCode = -1;
                return;
            }
            program = new Program(
                arguments[0],
                arguments[1],
                arguments[2]);
            parameters = new string[arguments.Length - 4];
            Array.Copy(arguments, 4, parameters, 0, parameters.Length);
            try
            {
                Environment.ExitCode = program.Execute(
                    Environment.CurrentDirectory,
                    arguments[3],
                    new CommandLineBatchParameter(parameters));
            }
            catch (Exception throwable)
            {
                Console.Error.WriteLine(throwable);
                Environment.ExitCode = -1;
                return;
            }
        }

    }

}
