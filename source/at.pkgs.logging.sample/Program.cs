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
using System.IO;
using At.Pkgs.Logging;
using At.Pkgs.Logging.Configuration;
using At.Pkgs.Logging.Rule;
using At.Pkgs.Logging.Sink;

namespace At.Pkgs.Logging.Sample
{

    public class Program
    {

        private readonly LogManager _manager;

        private readonly Log _log;

        protected LogManager LogManager
        {
            get
            {
                return this._manager;
            }
        }

        protected Log Log
        {
            get
            {
                return this._log;
            }
        }

        /*
         * thisを使用すれば基底クラスで共通処理としてインスタンス取得可能.
         */
        protected Program()
        {
            this._manager = new LogManager();
            this._log = this.LogManager.LogFor(this);
        }

        /*
         * String.Format()相当のフォーマッタ
         */
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

        /*
         * Appender:
         * 各種ラッパを組み合わせて機能を拡張できる
         */
        protected void SynchronizedAutoFlushDiagnosticsDebugAppender()
        {
            this.LogManager.Appender =
                new AutoFlush(
                    new Synchronized(
                        new DiagnosticsDebugAppender()));
            this.WriteLog();
        }

        /*
         * Appender:
         * 各種ラッパを組み合わせて機能を拡張できる
         */
        protected void SynchronizedConsoleAppender()
        {
            this.LogManager.Appender =
                new Synchronized(
                    new ConsoleAppender());
            this.WriteLog();
            /*
2013-11-11T13:12:11.884 (0)-0 NOTICE  At.Pkgs.Logging.Sample.Program notice log with format
CallStack:

2013-11-11T13:12:11.885 (0)-0 ERROR   At.Pkgs.Logging.Sample.Program error log with exception
by System.Exception: exception message
   at At.Pkgs.Logging.Sample.Program.WriteLog() in C:\com.unitystack\source\at.pkgs.logging.sample\Program.cs:line 58
CallStack:

2013-11-11T13:12:11.886 (0)-0 FATAL   At.Pkgs.Logging.Sample.Program fatal log with exception and format
by System.Exception: exception message
   at At.Pkgs.Logging.Sample.Program.WriteLog() in C:\com.unitystack\source\at.pkgs.logging.sample\Program.cs:line 66
CallStack:

             */
        }

        /*
         * LogManager:
         * 機能ごとのOn/Off、設定変更は動的に反映される
         */
        protected void ChangeLogManagerSettings()
        {
            this.Log.Notice("normal");
            /*
2013-11-11T13:12:11.887 (0)-0 NOTICE  At.Pkgs.Logging.Sample.Program normal
CallStack:

             */
            this.LogManager.LogProcessId = true;
            this.Log.Notice("set LogProcessId: true");
            /*
2013-11-11T13:12:11.887 (10812)-0 NOTICE  At.Pkgs.Logging.Sample.Program set LogProcessId: true
CallStack:

             */
            this.LogManager.LogManagedThreadId = true;
            this.Log.Notice("set LogManagedThreadId: true");
            /*
2013-11-11T13:12:11.888 (10812)-1 NOTICE  At.Pkgs.Logging.Sample.Program set LogManagedThreadId: true
CallStack:

             */
            this.LogManager.LogFrameDepth = 8;
            this.Log.Notice("set LogFrameDepth: 8");
            /*
2013-11-11T13:12:11.888 (10812)-1 NOTICE  At.Pkgs.Logging.Sample.Program set LogFrameDepth: 8
CallStack:
 from At.Pkgs.Logging.Sample.Program::ChangeLogManagerSettings() in :line 0
 from At.Pkgs.Logging.Sample.Program::Main() in :line 0

             */
            this.LogManager.LogExtendedFrame = true;
            this.Log.Notice("set LogExtendedFrame: true");
            /*
2013-11-11T13:12:11.888 (10812)-1 NOTICE  At.Pkgs.Logging.Sample.Program set LogExtendedFrame: true
CallStack:
 from At.Pkgs.Logging.Sample.Program::ChangeLogManagerSettings() in C:\com.unitystack\source\at.pkgs.logging.sample\Program.cs:line 113
 from At.Pkgs.Logging.Sample.Program::Main() in C:\com.unitystack\source\at.pkgs.logging.sample\Program.cs:line 239

             */
        }

        /*
         * ログフォーマットについて:
         * 基本はString.Format()
         * インデックスの代わりにenumに定義した文字列が使用可能に拡張
         * 参照: At.Pkgs.Logging.Sink.FormatAppender.*FormatWords
         */
        protected void ChangeFormat()
        {
            FormatAppender formatter;

            formatter = (FormatAppender)this.LogManager.Appender.Unwrap(typeof(FormatAppender));
            if (formatter != null)
            {
                formatter.MessageFormat =
                    "{Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff} {LevelName,-7} {SourceName} {Message}{NewLine}{Causes}";
            }
            this.WriteLog();
            /*
2013-11-11T13:12:11.891 NOTICE  At.Pkgs.Logging.Sample.Program notice log with format
2013-11-11T13:12:11.892 ERROR   At.Pkgs.Logging.Sample.Program error log with exception
by System.Exception: exception message
   at At.Pkgs.Logging.Sample.Program.WriteLog() in C:\com.unitystack\source\at.pkgs.logging.sample\Program.cs:line 58
2013-11-11T13:12:11.892 FATAL   At.Pkgs.Logging.Sample.Program fatal log with exception and format
by System.Exception: exception message
   at At.Pkgs.Logging.Sample.Program.WriteLog() in C:\com.unitystack\source\at.pkgs.logging.sample\Program.cs:line 66
             */
        }

        /*
         * LogLevelの制御もLogごとに動的に変更可能
         * 
         * パターンについて:
         * '*'は'.'を含む0個以上の文字にマッチ
         * '-'は'.'を含まない0個以上の文字にマッチ
         * 
         * Rule/LogMatchers.And, Or, Not等を使用して複雑な条件も指定可能
         */
        protected void LogLevelResolver()
        {
            Log a;
            Log b;
            List<LogLevelResolver> resolvers;

            a = this.LogManager.LogFor("At.Pkgs.Logging.Sample.SomeAction");
            b = this.LogManager.LogFor("Jp.Architector.Sample.MoreAction");
            resolvers = new List<LogLevelResolver>();
            this.Log.Notice("normal");
            a.Debug("ng");
            b.Debug("ng");
            /*
2013-11-11T13:12:11.894 NOTICE  At.Pkgs.Logging.Sample.Program normal
             */
            this.Log.Notice("set Debug for all(*)");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("*"),
                LogLevel.Debug));
            this.LogManager.Update(resolvers.ToArray());
            a.Debug("ok");
            b.Debug("ok");
            /*
2013-11-11T13:12:11.894 NOTICE  At.Pkgs.Logging.Sample.Program set Debug for all(*)
2013-11-11T13:12:11.896 DEBUG   At.Pkgs.Logging.Sample.SomeAction ok
2013-11-11T13:12:11.897 DEBUG   Jp.Architector.Sample.MoreAction ok
             */
            this.Log.Notice("set Notice for Jp.Architector.*");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("Jp.Architector.*"),
                LogLevel.Notice));
            this.LogManager.Update(resolvers.ToArray());
            a.Debug("ok");
            b.Debug("ng");
            /*
2013-11-11T13:12:11.897 NOTICE  At.Pkgs.Logging.Sample.Program set Notice for Jp.Architector.*
2013-11-11T13:12:11.897 DEBUG   At.Pkgs.Logging.Sample.SomeAction ok
             */
            this.Log.Notice("set Trace for *Action");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("*Action"),
                LogLevel.Trace));
            this.LogManager.Update(resolvers.ToArray());
            a.Trace("ok");
            b.Trace("ok");
            this.Log.Trace("ng");
            /*
2013-11-11T13:12:11.897 NOTICE  At.Pkgs.Logging.Sample.Program set Trace for *Action
2013-11-11T13:12:11.898 TRACE   At.Pkgs.Logging.Sample.SomeAction ok
2013-11-11T13:12:11.898 TRACE   Jp.Architector.Sample.MoreAction ok
             */
            this.Log.Notice("set Debug for *.-reAction");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("*.-reAction"),
                LogLevel.Debug));
            this.LogManager.Update(resolvers.ToArray());
            a.Trace("ok");
            b.Trace("ng");
            this.Log.Trace("ng");
            /*
2013-11-11T13:12:11.898 NOTICE  At.Pkgs.Logging.Sample.Program set Debug for *.-reAction
2013-11-11T13:12:11.899 TRACE   At.Pkgs.Logging.Sample.SomeAction ok
             */
        }

        /*
         * Appenderパイプラインの例
         */
        protected void AppenderPipeline()
        {
            Tee tee;
            FormatAppender formatter;

            this.LogManager.Appender =
                new Synchronized(
                    new Tee(
                        new ConsoleAppender(),
                        new NullAppender()));
            tee = (Tee)this.LogManager.Appender.Unwrap(typeof(Tee));
            if (tee == null)
            {
                this.Log.Fatal("failed on unwrap tee");
                return;
            }
            formatter = (FormatAppender)tee[0].Unwrap(typeof(FormatAppender));
            if (formatter != null)
                formatter.MessageFormat = "Appender#0 {Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff} {LevelName,-7} {SourceName} {Message}{NewLine}{Causes}";
            this.Log.Notice("ConsoleAppender and NullAppender");
            /*
Appender#0 2013-11-11T13:12:11.901 NOTICE  At.Pkgs.Logging.Sample.Program ConsoleAppender and NullAppender
             */
            tee[1] = new Filter(
                new ConsoleAppender(),
                LogEntityMatchers.And(
                    LogEntityMatchers.LevelHigherThan(LogLevel.Notice),
                    LogEntityMatchers.SourceMatches(
                        LogMatchers.NameMatchesPattern("At.Pkgs.Logging.Sample.*"))));
            formatter = (FormatAppender)tee[1].Unwrap(typeof(FormatAppender));
            if (formatter != null)
                formatter.MessageFormat = "Appender#1 {Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff} {LevelName,-7} {SourceName} {Message}{NewLine}{Causes}";
            foreach (Appender appender in tee)
            {
                formatter = (FormatAppender)appender.Unwrap(typeof(FormatAppender));
                if (formatter == null)
                    this.Log.Fatal("failed on unwap formatter");
                else
                    this.Log.Notice("message format: {0}", formatter.MessageFormat);
            }
            /*
Appender#0 2013-11-11T13:12:11.904 NOTICE  At.Pkgs.Logging.Sample.Program message format: Appender#0 {Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff} {LevelName,-7} {SourceName} {Message}{NewLine}{Causes}
Appender#0 2013-11-11T13:12:11.905 NOTICE  At.Pkgs.Logging.Sample.Program message format: Appender#1 {Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff} {LevelName,-7} {SourceName} {Message}{NewLine}{Causes}
             */
            this.Log.Notice("single appender");
            /*
Appender#0 2013-11-11T13:12:11.906 NOTICE  At.Pkgs.Logging.Sample.Program single appender
             */
            this.Log.Error("multipul appenders");
            /*
Appender#0 2013-11-11T13:12:11.906 ERROR   At.Pkgs.Logging.Sample.Program multipul appenders
Appender#1 2013-11-11T13:12:11.906 ERROR   At.Pkgs.Logging.Sample.Program multipul appenders
             */
        }

        protected void BasicLoggingConfiguration()
        {
            BasicLoggingConfiguration configuration;
            Stream stream;

            configuration = new BasicLoggingConfiguration(this.LogManager);
            stream = System.IO.File.OpenRead("Logging1.xml");
            try
            {
                configuration.Configure(stream);
            }
            finally
            {
                stream.Close();
            }
            this.Log.Debug("ng");
            this.Log.Notice("ok");
            stream = System.IO.File.OpenRead("Logging2.xml");
            try
            {
                configuration.Configure(stream);
            }
            finally
            {
                stream.Close();
            }
            this.Log.Trace("ng");
            this.Log.Debug("ok");
        }

        public static void Main(string[] arguments)
        {
            Program instance;

            instance = new Program();
            instance.SynchronizedAutoFlushDiagnosticsDebugAppender();
            instance.SynchronizedConsoleAppender();
            instance.ChangeLogManagerSettings();
            instance.ChangeFormat();
            instance.LogLevelResolver();
            instance.AppenderPipeline();
            instance.BasicLoggingConfiguration();
            Console.Write("press ENTER to exit...");
            Console.In.ReadLine();
        }

    }

}
