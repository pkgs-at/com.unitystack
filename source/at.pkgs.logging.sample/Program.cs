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
using At.Pkgs.Logging;
using At.Pkgs.Logging.Sink;

namespace At.Pkgs.Logging.Sample
{

    public class Program
    {

        private readonly Log _log;

        protected Log Log
        {
            get
            {
                return this._log;
            }
        }

        protected Program()
        {
            this._log = LogManager.Instance.LogFor(this);
        }

        private void WriteLog()
        {
            this.Log.Trace("trace log");
            this.Log.Debug("debug log");
            this.Log.Notice("notice log with {0}", "format");
            try
            {
                throw new Exception("exception message");
            }
            catch (Exception throwable)
            {
                this.Log.Error(throwable, "error log with exception");
            }
            try
            {
                throw new Exception("exception message");
            }
            catch (Exception throwable)
            {
                this.Log.Fatal(throwable, "fatal log with exception and {0}", "format");
            }
        }

        protected void SynchronizedAutoFlushDiagnosticsDebugAppender()
        {
            LogManager.Instance.Appender =
                new AutoFlush(
                    new Synchronized(
                        new DiagnosticsDebugAppender()));
            this.WriteLog();
        }

        protected void SynchronizedConsoleAppender()
        {
            LogManager.Instance.Appender =
                new Synchronized(
                    new ConsoleAppender());
            this.WriteLog();
        }

        protected void ChangeLogManagerSettings()
        {
            this.Log.Notice("normal");
            LogManager.Instance.LogProcessId = true;
            this.Log.Notice("set LogProcessId: true");
            LogManager.Instance.LogManagedThreadId = true;
            this.Log.Notice("set LogManagedThreadId: true");
            LogManager.Instance.LogFrameDepth = 8;
            this.Log.Notice("set LogFrameDepth: 8");
            LogManager.Instance.LogExtendedFrame = true;
            this.Log.Notice("set LogExtendedFrame: true");
        }

        protected void ChangeFormat()
        {
            FormatAppender formatter;

            formatter = (FormatAppender)LogManager.Instance.Appender.Unwrap(typeof(FormatAppender));
            if (formatter != null)
            {
                formatter.MessageFormat =
                    "{Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff} {LevelName,-7} {SourceName} {Message}{NewLine}{Causes}";
            }
            this.WriteLog();
        }

        public static void Main(string[] arguments)
        {
            Program instance;

            instance = new Program();
            instance.SynchronizedAutoFlushDiagnosticsDebugAppender();
            instance.SynchronizedConsoleAppender();
            instance.ChangeLogManagerSettings();
            instance.ChangeFormat();
            Console.Write("press ENTER to exit...");
            Console.In.ReadLine();
        }

    }

}
