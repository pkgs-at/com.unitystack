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

        public readonly LogManager LogManager;

        private readonly Log _log;

        protected Program()
        {
            this.LogManager = new LogManager();
            this._log = this.LogManager.LogFor(this);
        }

        private void WriteLog()
        {
            this._log.Trace("trace log");
            this._log.Debug("debug log");
            this._log.Notice("notice log with {0}", "format");
            try
            {
                throw new Exception("exception message");
            }
            catch (Exception throwable)
            {
                this._log.Error(throwable, "error log with exception");
            }
            try
            {
                throw new Exception("exception message");
            }
            catch (Exception throwable)
            {
                this._log.Fatal(throwable, "fatal log with exception and {0}", "format");
            }
        }

        protected void SynchronizedConsoleAppender()
        {
            this.LogManager.Appender =
                new Synchronized(
                    new ConsoleAppender());
            this.WriteLog();
        }

        protected void SynchronizedAutoFlushDiagnosticsDebugAppender()
        {
            this.LogManager.Appender =
                new AutoFlush(
                    new Synchronized(
                        new DiagnosticsDebugAppender()));
            this.WriteLog();
        }

        protected void ChangeFormat()
        {
            FormatAppender formatter;

            formatter = (FormatAppender)this.LogManager.Appender.Unwrap(typeof(FormatAppender));
            if (formatter != null)
            {
                formatter.MessageFormat =
                    "{1:yyyy-MM-dd'T'HH:mm:dd.fff} {3,-7} {2} {9}{0}{10}";

            }
            this.WriteLog();
        }

        public static void Main(string[] arguments)
        {
            Program instance;

            instance = new Program();
            instance.SynchronizedConsoleAppender();
            instance.ChangeFormat();
            instance.SynchronizedAutoFlushDiagnosticsDebugAppender();
            instance.ChangeFormat();
            Console.Write("press ENTER to exit...");
            Console.In.ReadLine();
        }

    }

}
