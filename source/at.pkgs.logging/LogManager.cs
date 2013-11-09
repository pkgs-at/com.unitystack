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
using System.Collections.Generic;
using At.Pkgs.Logging.Sink;
using At.Pkgs.Logging.Rule;

namespace At.Pkgs.Logging
{

    public class LogManager
    {

        private Appender _appender;

        private LogLevelResolver[] _resolvers;

        private Dictionary<String, Log.Shadow> _logs;

        private ReaderWriterLock _lock;

        public LogManager()
        {
            this._appender = new NullAppender();
            this._resolvers = new LogLevelResolver[0];
            this._logs = new Dictionary<String, Log.Shadow>();
            this._lock = new ReaderWriterLock();
        }

        public Appender Appender
        {
            get
            {
                return this._appender;
            }
            set
            {
                Appender old;

                if (value == null) throw new ArgumentNullException();
                lock (this)
                {
                    old = this._appender;
                    this._appender = value;
                }
                if (old == value) return;
                old.Flush();
                old.Close();
            }
        }

        internal void Add(Log.Shadow shadow)
        {
            LockCookie cookie;

            cookie = this._lock.UpgradeToWriterLock(Timeout.Infinite);
            try
            {
                this._logs.Add(shadow.Instance.Name, shadow);
            }
            finally
            {
                this._lock.DowngradeFromWriterLock(ref cookie);
            }
        }

        public void Append(LogEntity entity)
        {
            this.Appender.Append(entity);
        }

        protected LogLevel LevelFor(Log log)
        {
            LogLevel level;

            level = LogLevel.Notice;
            this._lock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                foreach (LogLevelResolver resolver in this._resolvers)
                {
                    LogLevel? result;

                    result = resolver.Resolve(log);
                    if (result.HasValue) level = result.Value;
                }
            }
            finally
            {
                this._lock.ReleaseReaderLock();
            }
            return level;
        }

        public void Update(LogLevelResolver[] resolvers)
        {
            LockCookie cookie;

            if (resolvers == null) throw new ArgumentNullException();
            cookie = this._lock.UpgradeToWriterLock(Timeout.Infinite);
            try
            {
                this._resolvers = resolvers;
            }
            finally
            {
                this._lock.DowngradeFromWriterLock(ref cookie);
            }
            this._lock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                foreach (Log.Shadow shadow in this._logs.Values)
                    shadow.Level = this.LevelFor(shadow.Instance);
            }
            finally
            {
                this._lock.ReleaseReaderLock();
            }
        }

        public Log LogFor(string name)
        {
            if (name == null) throw new ArgumentNullException();
            this._lock.AcquireReaderLock(Timeout.Infinite);
            try
            {
                if (!this._logs.ContainsKey(name))
                {
                    Log log;

                    log = new Log(this, name);
                    this._logs[name].Level = this.LevelFor(log);
                }
                return this._logs[name].Instance;
            }
            finally
            {
                this._lock.ReleaseReaderLock();
            }
        }

        public Log LogFor(Object target)
        {
            if (target == null) throw new ArgumentNullException();
            return this.LogFor(target.GetType().FullName);
        }

    }

}
