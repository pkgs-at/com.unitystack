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

namespace At.Pkgs.Logging
{

    public class Log
    {

        private WeakReference _manager;

        private string _name;

        private LogLevel _level;

        public Log(LogManager manager, string name)
        {
            this._manager = new WeakReference(manager);
            this._name = name;
            this._level = LogLevel.None;
        }

        public string Name
        {
            get
            {
                return this._name;
            }
        }

        public LogLevel Level
        {
            get
            {
                return this._level;
            }
            set
            {
                this._level = value;
            }
        }

        public bool Enabled(LogLevel level)
        {
            return level <= this._level;
        }

        public void Append(int depth, LogLevel level, Exception throwable, string format, params object[] arguments)
        {
            LogManager manager;
            LogEntity entity;

            if (level > this._level) return;
            if (!this._manager.IsAlive) return;
            manager = (LogManager)this._manager.Target;
            entity = new LogEntity();
            entity.Timestamp = DateTime.Now;
            entity.Source = this.Name;
            entity.Level = level;
            entity.Frame = new StackFrame(++depth, true);
            entity.Message = arguments.Length <= 0 ? format : String.Format(format, arguments);
            entity.Cause = throwable;
            manager.Append(entity);
        }

        public bool TraceEnabled
        {
            get
            {
                LogLevel level = LogLevel.Trace;

                return level <= this._level;
            }
        }

        public void Trace(string message)
        {
            LogLevel level = LogLevel.Trace;

            if (level > this._level) return;
            this.Append(1, level, null, message);
        }

        public void Trace(string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Trace;

            if (level > this._level) return;
            this.Append(1, level, null, format, arguments);
        }

        public void Trace(Exception cause, string message)
        {
            LogLevel level = LogLevel.Trace;

            if (level > this._level) return;
            this.Append(1, level, cause, message);
        }

        public void Trace(Exception cause, string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Trace;

            if (level > this._level) return;
            this.Append(1, level, cause, format, arguments);
        }

        public bool DebugEnabled
        {
            get
            {
                LogLevel level = LogLevel.Debug;

                return level <= this._level;
            }
        }

        public void Debug(string message)
        {
            LogLevel level = LogLevel.Debug;

            if (level > this._level) return;
            this.Append(1, level, null, message);
        }

        public void Debug(string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Debug;

            if (level > this._level) return;
            this.Append(1, level, null, format, arguments);
        }

        public void Debug(Exception cause, string message)
        {
            LogLevel level = LogLevel.Debug;

            if (level > this._level) return;
            this.Append(1, level, cause, message);
        }

        public void Debug(Exception cause, string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Debug;

            if (level > this._level) return;
            this.Append(1, level, cause, format, arguments);
        }

        public bool NoticeEnabled
        {
            get
            {
                LogLevel level = LogLevel.Notice;

                return level <= this._level;
            }
        }

        public void Notice(string message)
        {
            LogLevel level = LogLevel.Notice;

            if (level > this._level) return;
            this.Append(1, level, null, message);
        }

        public void Notice(string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Notice;

            if (level > this._level) return;
            this.Append(1, level, null, format, arguments);
        }

        public void Notice(Exception cause, string message)
        {
            LogLevel level = LogLevel.Notice;

            if (level > this._level) return;
            this.Append(1, level, cause, message);
        }

        public void Notice(Exception cause, string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Notice;

            if (level > this._level) return;
            this.Append(1, level, cause, format, arguments);
        }

        public bool ErrorEnabled
        {
            get
            {
                LogLevel level = LogLevel.Error;

                return level <= this._level;
            }
        }

        public void Error(string message)
        {
            LogLevel level = LogLevel.Error;

            if (level > this._level) return;
            this.Append(1, level, null, message);
        }

        public void Error(string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Error;

            if (level > this._level) return;
            this.Append(1, level, null, format, arguments);
        }

        public void Error(Exception cause, string message)
        {
            LogLevel level = LogLevel.Error;

            if (level > this._level) return;
            this.Append(1, level, cause, message);
        }

        public void Error(Exception cause, string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Error;

            if (level > this._level) return;
            this.Append(1, level, cause, format, arguments);
        }

        public bool FatalEnabled
        {
            get
            {
                LogLevel level = LogLevel.Fatal;

                return level <= this._level;
            }
        }

        public void Fatal(string message)
        {
            LogLevel level = LogLevel.Fatal;

            if (level > this._level) return;
            this.Append(1, level, null, message);
        }

        public void Fatal(string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Fatal;

            if (level > this._level) return;
            this.Append(1, level, null, format, arguments);
        }

        public void Fatal(Exception cause, string message)
        {
            LogLevel level = LogLevel.Fatal;

            if (level > this._level) return;
            this.Append(1, level, cause, message);
        }

        public void Fatal(Exception cause, string format, params object[] arguments)
        {
            LogLevel level = LogLevel.Fatal;

            if (level > this._level) return;
            this.Append(1, level, cause, format, arguments);
        }

    }

}
