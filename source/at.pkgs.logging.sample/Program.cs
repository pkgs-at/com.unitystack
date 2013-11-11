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
using At.Pkgs.Logging;
using At.Pkgs.Logging.Rule;
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

        /*
         * thisを使用すれば基底クラスで共通処理としてインスタンス取得可能.
         */
        protected Program()
        {
            this._log = LogManager.Instance.LogFor(this);
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
            LogManager.Instance.Appender =
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
            LogManager.Instance.Appender =
                new Synchronized(
                    new ConsoleAppender());
            this.WriteLog();
        }

        /*
         * LogManager:
         * 機能ごとのOn/Off、設定変更は動的に反映される
         */
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

        /*
         * ログフォーマットについて:
         * 基本はString.Format()
         * インデックスの代わりにenumに定義した文字列が使用可能に拡張
         * 参照: At.Pkgs.Logging.Sink.FormatAppender.*FormatWords
         */
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

            a = LogManager.Instance.LogFor("At.Pkgs.Logging.Sample.SomeAction");
            b = LogManager.Instance.LogFor("Jp.Architector.Sample.MoreAction");
            resolvers = new List<LogLevelResolver>();
            this.Log.Notice("normal");
            a.Debug("ng");
            b.Debug("ng");
            this.Log.Notice("set Debug for all(*)");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("*"),
                LogLevel.Debug));
            LogManager.Instance.Update(resolvers.ToArray());
            a.Debug("ok");
            b.Debug("ok");
            this.Log.Notice("set Notice for Jp.Architector.*");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("Jp.Architector.*"),
                LogLevel.Notice));
            LogManager.Instance.Update(resolvers.ToArray());
            a.Debug("ok");
            b.Debug("ng");
            this.Log.Notice("set Trace for *Action");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("*Action"),
                LogLevel.Trace));
            LogManager.Instance.Update(resolvers.ToArray());
            a.Trace("ok");
            b.Trace("ok");
            this.Log.Trace("ng");
            this.Log.Notice("set Debug for *.-reAction");
            resolvers.Add(LogLevelResolvers.LogMatches(
                LogMatchers.NameMatchesPattern("*.-reAction"),
                LogLevel.Debug));
            LogManager.Instance.Update(resolvers.ToArray());
            a.Trace("ok");
            b.Trace("ng");
            this.Log.Trace("ng");
        }

        /*
         * Appenderパイプラインの例
         */
        protected void AppenderPipeline()
        {
            Tee tee;
            FormatAppender formatter;

            LogManager.Instance.Appender =
                new Synchronized(
                    new Tee(
                        new ConsoleAppender(),
                        new NullAppender()));
            tee = (Tee)LogManager.Instance.Appender.Unwrap(typeof(Tee));
            if (tee == null)
            {
                this.Log.Fatal("failed on unwrap tee");
                return;
            }
            formatter = (FormatAppender)tee[0].Unwrap(typeof(FormatAppender));
            if (formatter != null)
                formatter.MessageFormat = "Appender#0 {Timestamp:yyyy-MM-dd'T'HH:mm:dd.fff} {LevelName,-7} {SourceName} {Message}{NewLine}{Causes}";
            this.Log.Notice("ConsoleAppender and NullAppender");
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
            this.Log.Notice("single appender");
            this.Log.Error("multipul appenders");
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
            Console.Write("press ENTER to exit...");
            Console.In.ReadLine();
        }

    }

}
